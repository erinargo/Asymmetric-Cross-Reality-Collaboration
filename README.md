# To Start
Clone to directory. Then, in UnityHub, Add -> ${dir} -> Select root folder. 
Should start importing all dependencies automatically.

# Code Style

# Version Control
Work in branches. Create a new branch for each new feature. Each branch should have the name of the programmer, name of feature, and date.
Submit for review and ping either Dr. Grandi or Erin for approval.

# Helpful Links
## Getting Started with NetCode for GameObjects
Unity Docs: https://docs-multiplayer.unity3d.com/netcode/current/about/

Tutorial for multiplayer environment setup using NetCode + Relay 
(not necessary to follow, this framework has already been implemented with NetCode + Relay + Lobbies): https://www.youtube.com/watch?v=fAnaQTGVs4I

The most important thing to remember about NetCode is that the p2p connection created treats hosts and clients with very 
strict and often confusing rules. Outlined below are a list of things to keep in mind when implementing changes to a multiplayer game:

1. Each instance of any game client is a single player instance of the multiplayer game 

   2. What does this mean?
   
   3. Each client has a local copy of all game objects and scripts, we introduce multiplayer when we sync specific game objects with
   special multiplayer scripts across the network
   
   4. Unity classifies a singleplayer script as MonoBehaviour which your script will derive from
    
      5. E.g.  
     
      ```C#
      public class YourClass : MonoBehaviour
      {
       // Blah
      }
      ```
      
   6. Unity classifies a multiplayer script as a NetworkBehaviour which your script will derive from
   
      7. E.g.
      
      ```C#
      public class YourClass : NetworkBehaviour
      {
       // Blah
      }
      ```
      
8. NetworkObjects
 
   9. Any gameobject that has a script attached to it that derives from NetworkBehaviour also needs a NetworkObject script attached to it as well
       
      11. Unity provides this NetworkObject Script for you. If you don't add it before attaching a NetworkBehaviour derived script, don't worry, the editor will tell you.
    
   8. Any gameobject with a NetworkObject script attached to it has to be added to the network prefabs list. Usually, this is done automatically for you but not always. 
    
   9. $${\color{red}!!!!!!!!!PAY ATTENTION!!!!!!!!!}$$
       
      10. YOU WILL EXPERIENCE TERRIBLE AND MIND BOGGLING BUGS IF YOU DO NOT ADD A NETWORKOBJECT TO THE NETWORK PREFAB LIST. 
      
      11. SEEMINGLY UNRELATED THINGS THAT WERE WORKING BEFORE MIGHT SUDDENLY AND COMPLETELY AND INEXPLICABLY BREAK.
       
      12. YOUR NEW IMPLEMENTED CODE WILL NOT WORK.
      
      13. DOWN THIS PATH LIES MADNESS.
      
      11. DO NOT DESPAIR.
       
      12. TAKE A BREAK.
       
      13. TEXT ERIN BEFORE SPENDING 48 HOURS OF YOUR EVER SHORTENING LIFE TRYING TO DEBUG SOMETHING THAT SHOULD ABSOLUTELY 1000000% BE WORKING.
      
      14. WE CAN PERSEVERE. TOGETHER.
 
3. The host is both the server and a client.
    
   3. For example, the host has the ability to move another client's player object -- even when we don't want the host to do this.
    
   4. Be very careful when implementing interactivity that might just be overridden by the host
 
4. Ownership
    
   5. Default ownership for everything within the game (except for the client's own connection) is allocated to the Server
       
      6. Which is, remember, also the host and also a client
    
   7. We can change ownership.
    
   8. Ownership is what gives permission to clients to alter some gameobject within the scene
       
      9. i.e. position, rotation, other properties like size and parenting
    
   10. Even when the client has ownership over an object, the host can override the client's interactions (unintentionally)
    
      10. To avoid this kind of harmful interaction, we can check if any given client (the host is also a client) has ownership of any given gameobject.
      Netcode exposes this data to the programmer with `IsOwner`
       
   12. `IsOwner` is intrinsically tied to the gameobject it is attached to through the NetworkObject script. 
    
      12. Other useful ownership keywords are `IsHost` and `IsServer`
    
4. How do we sync data across the network in a secure way?
 
   4. NetworkVariables!
    
      5. Data structures that can be read across the network
      
      6. NetworkVariables can only be Serializable data (i.e. numbers)
       
      6. There are ways to work around this but they're outside the scope of this readme, ask Erin if you need help with this
       
   5. For example, imagine you want to sync a player's xyz position. 
    
      6. First, set up your networked class.
       
      ```C#
      public class SyncGameObjectPosition : NetworkBehaviour
      {
       // Blah
      }
      ```
      
      7. Then, set up some NetworkedVariables. Think of all the data you want to share across the Network.
      
         8. In this case, we really only care about object position and object rotation and nothing else.
          
      ```C#
      public class SyncGameObjectPosition : NetworkBehaviour
      {
       // We only want owner of gameobject to be able to write but we want everyone to be able to read
       private NetworkVariable<Vector3> _netPos = new(writePerm: NetworkVariableWritePermission.Owner);
       private NetworkVariable<Quaternion> _netRot = new(writePerm: NetworkVariableWritePermission.Owner);
      }
      ```
      
      8. Now, we want to write some data to these variables. In this case, the data structure stores relevant values in 
      an exposed .Value variable. 
       
      ```C#
      public class SyncGameObjectPosition : NetworkBehaviour
      {
       // We only want owner of gameobject to be able to write but we want everyone to be able to read
       private NetworkVariable<Vector3> _netPos = new(writePerm: NetworkVariableWritePermission.Owner);
       private NetworkVariable<Quaternion> _netRot = new(writePerm: NetworkVariableWritePermission.Owner);
       void Update()
       {
      
        // Putting this in update is potentially laggy but useful for example
        if (IsOwner) // We want to check if owner to avoid overwriting data with host
        {
         // Always read and write from .Value, the editor will scream at you otherwise
         _netPos.Value = transform.position;
         _netRot.Value = transform.rotation;
        } 
      
       }
      }
      ```
      
      9. Then, if client is not the owner, we simply want to position the game object in the scene according to the data stored in the network variables.
      
      ```C#
      public class SyncGameObjectPosition : NetworkBehaviour
      {
       // We only want owner of gameobject to be able to write but we want everyone to be able to read
       private NetworkVariable<Vector3> _netPos = new(writePerm: NetworkVariableWritePermission.Owner);
       private NetworkVariable<Quaternion> _netRot = new(writePerm: NetworkVariableWritePermission.Owner);
       void Update()
       {
      
        // Putting this in update is potentially laggy but useful for example
        if (IsOwner)
        {
         // Always read and write from value, the editor will scream at you otherwise
         _netPos.Value = transform.position;
         _netRot.Value = transform.rotation;
        } else if (!IsOwner) // seems redundant. Isn't. They could also be IsServer or IsHost which might lead to unintended behaviour. 
        {
         transform.position = _netPos.Value;
         transform.rotation = _netRot.Value;
        }
      
       }
      }
      ```
      
      10. That's it! 
      
11. RPCs
 
    12. No idea what it stands for.
     
    13. Two kinds.
     
        14. ServerRPC
         
        15. ClientRPC
         
    16. This is used as a sort of API endpoint solution for server/client connection. If a client doesn't have permission to perform a necessary 
      operation, we can ask the server to do it for us. Vice versa, if a server wants to tell the client to do something then we can call a ClientRPC.
     
    18. General Structure
    
      ```C#
      ...
      private NetworkVariable<ulong> WinnerClientId = new(writePerm: NetworkVariableWritePermission.Server); 
    
      // Ownership of GameObject attached to not required to call this RPC method. 
      [ServerRpc(RequireOwnership = false)] // This needs to be declared.
      // Name of function must end with ServerRpc
      public void SetWinnerServerRpc(ulong clientId, ServerRpcParams serverRpcParams = default) { 
    // ServerRpcParams must be declared.
        WinnerClientId = clientId;
      }
      ...
      ```
    
11. The final topic in this quickstart to netcode is maybe a little advanced for now but will be necessary down the road. 
 
    12. Sometimes, you will need to store a lot of data in an entire class that needs to be networked. 
     
    13. This can be done with the `INetworkSerializable, IEquatable<T>` derivation. 
     
    14. All data within this class needs to be Serializable data (i.e. numbers) 
     
    15. If you need more information about this topic please refer to the guide, the following code example, or ask Erin
     
```C#
public class GameManager : NetworkBehaviour {
   [Serializable] // Needs to be declared for the editor
   public class PlayerData : INetworkSerializable, IEquatable<PlayerData>
   {
      private ulong ClientId;
      private int Score;
      
      public PlayerData(ulong clientId, int score = 0) 
      { // init
         ClientId = clientId;
         Score = score;
      }
      
      public int Score => Score;
      public ulong ClientId => ClientId;
      
      public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter {
         serializer.SerializeValue(ref ClientId);
         serializer.SerializeValue(ref Score);
     }
   
     public bool Equals(PlayerData other) {
         return ClientId == other.ClientId && Score == other.Score;
     }
   }
   
   // We can then use PlayerData to store a list of players
   
   private NetworkList<PlayerData> players; // Doesn't need to be a NetworkList but for this example I want to show that it can be.
   /**
      The rest of this code would be written as normal. NetworkList works like the normal List structure.
   */
}
```