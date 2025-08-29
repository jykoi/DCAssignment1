using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace InterfaceLibrary
{
    [DataContract]
    public class ChatMessage
    {
        [DataMember] public int Id { get; set; }           
        [DataMember] public string FromUser { get; set; }
        [DataMember] public string Text { get; set; }
        [DataMember] public DateTime Timestamp { get; set; }
    }

    [DataContract]
    public class MessagesPage
    {
        [DataMember] public List<ChatMessage> Items { get; set; }
        [DataMember] public int LastId { get; set; }       
        [DataMember] public bool HasMore { get; set; }     
    }
}
