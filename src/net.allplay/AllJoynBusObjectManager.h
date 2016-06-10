//-----------------------------------------------------------------------------
// <auto-generated> 
//   This code was generated by a tool. 
// 
//   Changes to this file may cause incorrect behavior and will be lost if  
//   the code is regenerated.
//
//   Tool: AllJoynCodeGenerator.exe
//
//   This tool is located in the Windows 10 SDK and the Windows 10 AllJoyn 
//   Visual Studio Extension in the Visual Studio Gallery.  
//
//   The generated code should be packaged in a Windows 10 C++/CX Runtime  
//   Component which can be consumed in any UWP-supported language using 
//   APIs that are available in Windows.Devices.AllJoyn.
//
//   Using AllJoynCodeGenerator - Invoke the following command with a valid 
//   Introspection XML file and a writable output directory:
//     AllJoynCodeGenerator -i <INPUT XML FILE> -o <OUTPUT DIRECTORY>
// </auto-generated>
//-----------------------------------------------------------------------------
#pragma once

class AllJoynBusObjectManager
{
public:
    // Retrieve the BusObject that is mapped to the specified BusAttachment and ObjectPath, or create one if it does not yet exist.
    static QStatus GetBusObject(_In_ const alljoyn_busattachment busAttachment, _In_ const PCSTR objectPath, _Out_ alljoyn_busobject* busObject);

    // Indicate to AllJoynBusObjectManager that a reference to the BusObject which is mapped to the specified BusAttachment and ObjectPath
    // is no longer being used. This should be invoked whenever a Consumer is destroyed.
    static QStatus ReleaseBusObject(_Inout_ alljoyn_busattachment busAttachment, _In_ const PCSTR objectPath);

    // If the BusObject is not already registered on the BusAttachment, it becomes registered. Otherwise, no action is taken.
    static QStatus TryRegisterBusObject(_Inout_ alljoyn_busattachment busAttachment, _In_ const alljoyn_busobject busObject, _In_ const bool secure);

    // Determine whether a BusObject with the specified ObjectPath has been mapped to the specified BusAttachment.
    static bool BusObjectExists(_In_ const alljoyn_busattachment busAttachment, _In_ const PCSTR objectPath);

    // Determine whether the specified BusObject is registered on the specified BusAttachment.
    static bool BusObjectIsRegistered(_In_ const alljoyn_busattachment busAttachment, _In_ const alljoyn_busobject busObject);

	// Save the given BusObject in BusAttachmentMap, keyed by busAttachment and objectPath.
	static QStatus SaveBusObject(_In_ const alljoyn_busattachment busAttachment, _In_ const PCSTR objectPath, _In_ const alljoyn_busobject busObject);
private:
    // Lock public methods that result in modifications to BusAttachmentMap to ensure thread-safety.
    static std::mutex ModifyBusObjectMap;

    // A nested map of BusObjects that is keyed first by BusAttachment, and then by ObjectPath, allowing a BusObject to be addressed 
    // by its ObjectPath and the BusAttachment on which it is registered.
    // Each innermost element (a tuple) contains three fields:
    //   (0) The BusObject
    //   (1) Whether the BusObject has been registered on the associated BusAttachment
    //   (2) The number of references to the entry
    static std::map<alljoyn_busattachment, std::shared_ptr<std::map<std::string, std::tuple<alljoyn_busobject, bool, int>>>> BusAttachmentMap;

    // Create a BusObject that has no callbacks and uses the specified ObjectPath.
    static QStatus CreateBusObject(_In_ const PCSTR objectPath, _Out_ alljoyn_busobject* busObject);

	// If the BusObject is registered on the specified BusAttachment, unregister it. Then destroy the BusObject.
    static QStatus UnregisterAndDestroyBusObject(_Inout_ alljoyn_busattachment busAttachment, _In_ const PCSTR objectPath);
};
