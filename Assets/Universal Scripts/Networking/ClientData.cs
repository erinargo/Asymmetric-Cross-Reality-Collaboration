using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ClientData {
    public ulong ClientId;
    // public int CharacterId;

    public ClientData(ulong clientId, int characterId = -1) { 
        this.ClientId = clientId;
        // this.CharacterId = characterId;
    }
}