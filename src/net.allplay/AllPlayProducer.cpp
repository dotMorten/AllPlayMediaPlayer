#include "pch.h"

using namespace concurrency;
using namespace Microsoft::WRL;
using namespace Platform;
using namespace Windows::Foundation;
using namespace Windows::Devices::AllJoyn;
using namespace net::allplay;
using namespace net::allplay::Control::Volume;
using namespace net::allplay::MediaPlayer;
using namespace net::allplay::MCU;
using namespace net::allplay::ZoneManager;


AllPlayProducer::AllPlayProducer(AllJoynBusAttachment^ busAttachment)
    : m_busAttachment(busAttachment),
    m_sessionListener(nullptr),
    m_busObject(nullptr),
    m_sessionPort(0),
    m_sessionId(0)
{
    m_weak = new WeakReference(this);
    ServiceObjectPath = ref new String(L"/net/allplay/MediaPlayer");
    m_busAttachmentStateChangedToken.Value = 0;
	m_mediaPlayerProducer = ref new MediaPlayerProducer(busAttachment);
	m_volumeProducer = ref new VolumeProducer(busAttachment);
	m_mcuProducer = ref new MCUProducer(busAttachment);
	m_zoneManagerProducer = ref new net::allplay::ZoneManager::ZoneManagerProducer(busAttachment);
}

AllPlayProducer::~AllPlayProducer()
{
    UnregisterFromBus();
    delete m_weak;
}

void AllPlayProducer::UnregisterFromBus()
{
    if ((nullptr != m_busAttachment) && (0 != m_busAttachmentStateChangedToken.Value))
    {
        m_busAttachment->StateChanged -= m_busAttachmentStateChangedToken;
        m_busAttachmentStateChangedToken.Value = 0;
    }
    if ((nullptr != m_busAttachment) && (nullptr != SessionPortListener))
    {
        alljoyn_busattachment_unbindsessionport(AllJoynHelpers::GetInternalBusAttachment(m_busAttachment), m_sessionPort);
        alljoyn_sessionportlistener_destroy(SessionPortListener);
        SessionPortListener = nullptr;
    }
    if ((nullptr != m_busAttachment) && (nullptr != BusObject))
    {
        alljoyn_busattachment_unregisterbusobject(AllJoynHelpers::GetInternalBusAttachment(m_busAttachment), BusObject);
        alljoyn_busobject_destroy(BusObject);
        BusObject = nullptr;
    }
    if (nullptr != SessionListener)
    {
        alljoyn_sessionlistener_destroy(SessionListener);
        SessionListener = nullptr;
    }
}

bool AllPlayProducer::OnAcceptSessionJoiner(_In_ alljoyn_sessionport sessionPort, _In_ PCSTR joiner, _In_ const alljoyn_sessionopts opts)
{
    UNREFERENCED_PARAMETER(sessionPort); UNREFERENCED_PARAMETER(joiner); UNREFERENCED_PARAMETER(opts);
    
    return true;
}

void AllPlayProducer::OnSessionJoined(_In_ alljoyn_sessionport sessionPort, _In_ alljoyn_sessionid id, _In_ PCSTR joiner)
{
    UNREFERENCED_PARAMETER(joiner);

    // We initialize the Signals object after the session has been joined, because it needs
    // the session id.
	
    Volume->InitializeSignals(id);
	MediaPlayer->InitializeSignals(id);
	MCU->InitializeSignals(id);
	ZoneManager->InitializeSignals(id);
	m_sessionPort = sessionPort;
    m_sessionId = id;

    alljoyn_sessionlistener_callbacks callbacks =
    {
        AllJoynHelpers::SessionLostHandler<AllPlayProducer>,
        AllJoynHelpers::SessionMemberAddedHandler<AllPlayProducer>,
        AllJoynHelpers::SessionMemberRemovedHandler<AllPlayProducer>
    };

    SessionListener = alljoyn_sessionlistener_create(&callbacks, m_weak);
    alljoyn_busattachment_setsessionlistener(AllJoynHelpers::GetInternalBusAttachment(m_busAttachment), id, SessionListener);
}

void AllPlayProducer::OnSessionLost(_In_ alljoyn_sessionid sessionId, _In_ alljoyn_sessionlostreason reason)
{
    if (sessionId == m_sessionId)
    {
        AllJoynSessionLostEventArgs^ args = ref new AllJoynSessionLostEventArgs(static_cast<AllJoynSessionLostReason>(reason));
        SessionLost(this, args);
    }
}

void AllPlayProducer::OnSessionMemberAdded(_In_ alljoyn_sessionid sessionId, _In_ PCSTR uniqueName)
{
    if (sessionId == m_sessionId)
    {
        auto args = ref new AllJoynSessionMemberAddedEventArgs(AllJoynHelpers::MultibyteToPlatformString(uniqueName));
        SessionMemberAdded(this, args);
    }
}

void AllPlayProducer::OnSessionMemberRemoved(_In_ alljoyn_sessionid sessionId, _In_ PCSTR uniqueName)
{
    if (sessionId == m_sessionId)
    {
        auto args = ref new AllJoynSessionMemberRemovedEventArgs(AllJoynHelpers::MultibyteToPlatformString(uniqueName));
        SessionMemberRemoved(this, args);
    }
}

void AllPlayProducer::BusAttachmentStateChanged(_In_ AllJoynBusAttachment^ sender, _In_ AllJoynBusAttachmentStateChangedEventArgs^ args)
{
    if (args->State == AllJoynBusAttachmentState::Connected)
    {   
        QStatus result = AllJoynHelpers::CreateProducerSession<AllPlayProducer>(m_busAttachment, m_weak);
        if (ER_OK != result)
        {
            StopInternal(result);
            return;
        }
    }
    else if (args->State == AllJoynBusAttachmentState::Disconnected)
    {
        StopInternal(ER_BUS_STOPPING);
    }
}

QStatus AllPlayProducer::AddMethodHandler(_In_ alljoyn_interfacedescription interfaceDescription, _In_ PCSTR methodName, _In_ alljoyn_messagereceiver_methodhandler_ptr handler)
{
    alljoyn_interfacedescription_member member;
    if (!alljoyn_interfacedescription_getmember(interfaceDescription, methodName, &member))
    {
        return ER_BUS_INTERFACE_NO_SUCH_MEMBER;
    }

    return alljoyn_busobject_addmethodhandler(
        m_busObject,
        member,
        handler,
        m_weak);
}

QStatus AllPlayProducer::AddSignalHandler(_In_ alljoyn_busattachment busAttachment, _In_ alljoyn_interfacedescription interfaceDescription, _In_ PCSTR methodName, _In_ alljoyn_messagereceiver_signalhandler_ptr handler)
{
    alljoyn_interfacedescription_member member;
    if (!alljoyn_interfacedescription_getmember(interfaceDescription, methodName, &member))
    {
        return ER_BUS_INTERFACE_NO_SUCH_MEMBER;
    }

    return alljoyn_busattachment_registersignalhandler(busAttachment, handler, member, NULL);
}

QStatus AllPlayProducer::OnPropertyGet(_In_ PCSTR interfaceName, _In_ PCSTR propertyName, _Inout_ alljoyn_msgarg value)
{
    UNREFERENCED_PARAMETER(interfaceName);
	
	if (0 == strcmp(interfaceName, "net.allplay.MediaPlayer"))
	{
		return MediaPlayer->OnPropertyGet(interfaceName, propertyName, value);
	}
	if (0 == strcmp(interfaceName, "org.alljoyn.Control.Volume"))
	{
		return Volume->OnPropertyGet(interfaceName, propertyName, value);
	}
	if (0 == strcmp(interfaceName, "net.allplay.MCU"))
	{
		return MCU->OnPropertyGet(interfaceName, propertyName, value);
	}
	if (0 == strcmp(interfaceName, "net.allplay.ZoneManager"))
	{
		return m_zoneManagerProducer->OnPropertyGet(interfaceName, propertyName, value);
	}
	return ER_BUS_NO_SUCH_PROPERTY;
}

QStatus AllPlayProducer::OnPropertySet(_In_ PCSTR interfaceName, _In_ PCSTR propertyName, _In_ alljoyn_msgarg value)
{
    UNREFERENCED_PARAMETER(interfaceName);
	if (0 == strcmp(interfaceName, "net.allplay.MediaPlayer"))
	{
		return MediaPlayer->OnPropertySet(interfaceName, propertyName, value);
	}
	if (0 == strcmp(interfaceName, "org.alljoyn.Control.Volume"))
	{
		return Volume->OnPropertySet(interfaceName, propertyName, value);
	}
	if (0 == strcmp(interfaceName, "net.allplay.MCU"))
	{
		return MCU->OnPropertySet(interfaceName, propertyName, value);
	}
	if (0 == strcmp(interfaceName, "net.allplay.ZoneManager"))
	{
		return m_zoneManagerProducer->OnPropertySet(interfaceName, propertyName, value);
	}
	return ER_BUS_NO_SUCH_PROPERTY;
}

void AllPlayProducer::Start()
{
	if (nullptr == m_busAttachment)
	{
		StopInternal(ER_FAIL);
		return;
	}

	QStatus result = AllJoynHelpers::CreateBusObject<AllPlayProducer>(m_weak);
	if (result != ER_OK)
	{
		StopInternal(result);
		return;
	}

	//alljoyn_busobject_addinterface_announced(BusObject, interfaceDescription);

	MediaPlayer->Start(BusObject);
	Volume->Start(BusObject);
	MCU->Start(BusObject);
	ZoneManager->Start(BusObject);

	//SourceObjects[m_busObject] = m_weak;
	//SourceInterfaces[interfaceDescription] = m_weak;

	unsigned int noneMechanismIndex = 0;
	bool authenticationMechanismsContainsNone = m_busAttachment->AuthenticationMechanisms->IndexOf(AllJoynAuthenticationMechanism::None, &noneMechanismIndex);
	QCC_BOOL interfaceIsSecure = false; //alljoyn_interfacedescription_issecure(interfaceDescription);

	// If the current set of AuthenticationMechanisms supports authentication,
	// determine whether a secure BusObject is required.
	if (AllJoynHelpers::CanSecure(m_busAttachment->AuthenticationMechanisms))
	{
		// Register the BusObject as "secure" if the org.alljoyn.Bus.Secure XML annotation
		// is specified, or if None is not present in AuthenticationMechanisms.
		if (!authenticationMechanismsContainsNone || interfaceIsSecure)
		{
			result = alljoyn_busattachment_registerbusobject_secure(AllJoynHelpers::GetInternalBusAttachment(m_busAttachment), BusObject);
		}
		else
		{
			result = alljoyn_busattachment_registerbusobject(AllJoynHelpers::GetInternalBusAttachment(m_busAttachment), BusObject);
		}
	}
	else
	{
		// If the current set of AuthenticationMechanisms does not support authentication
		// but the interface requires security, report an error.
		if (interfaceIsSecure)
		{
			result = ER_BUS_NO_AUTHENTICATION_MECHANISM;
		}
		else
		{
			result = alljoyn_busattachment_registerbusobject(AllJoynHelpers::GetInternalBusAttachment(m_busAttachment), BusObject);
		}
	}

	if (result != ER_OK)
	{
		StopInternal(result);
		return;
	}
	m_busAttachmentStateChangedToken = m_busAttachment->StateChanged += ref new TypedEventHandler<AllJoynBusAttachment^, AllJoynBusAttachmentStateChangedEventArgs^>(this, &AllPlayProducer::BusAttachmentStateChanged);
	m_busAttachment->Connect();
}

void AllPlayProducer::Stop()
{
    StopInternal(AllJoynStatus::Ok);
}

void AllPlayProducer::StopInternal(int32 status)
{
    UnregisterFromBus();
    Stopped(this, ref new AllJoynProducerStoppedEventArgs(status));
}

int32 AllPlayProducer::RemoveMemberFromSession(_In_ String^ uniqueName)
{
    return alljoyn_busattachment_removesessionmember(
        AllJoynHelpers::GetInternalBusAttachment(m_busAttachment),
        m_sessionId,
        AllJoynHelpers::PlatformToMultibyteString(uniqueName).data());
}

