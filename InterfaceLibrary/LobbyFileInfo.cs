using System;
using System.Runtime.Serialization;

namespace InterfaceLibrary
{
    [DataContract]
    public class LobbyFileInfo
    {
        [DataMember] public int Id { get; set; }
        [DataMember] public string FileName { get; set; }
        [DataMember] public string ContentType { get; set; }  // Can be image files or text files.
        [DataMember] public string UploadedBy { get; set; }
        [DataMember] public DateTime UploadedAt { get; set; }
    }
}