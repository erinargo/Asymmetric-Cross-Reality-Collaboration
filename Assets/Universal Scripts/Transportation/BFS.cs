using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BFS : MonoBehaviour
{
    public enum State {
        Inactive,
        Active
    }

    // Instantiate(GameObject, optional transform)

    public class Node {
        private Transform _Position;
        private State _State;
        private Color _RouteColor;
        
        private List<Node> _ConnectedNodes;

        public Node(Transform position, State state = State.Inactive) {
            this._Position = position;
            this._State = state;
            this._RouteColor = new Color(0, 0, 0, 0);
            
            _ConnectedNodes = new List<Node>();
        }

        public State GetState() => _State;
        public Transform GetPosition() => _Position;
        public List<Node> GetConnectedNodes() => _ConnectedNodes;

        public void AddConnection(Node node) {
            _ConnectedNodes.Add(node);
        }
        
        public void RemoveConnection(Node node) {
            _ConnectedNodes.Remove(node);
        }

        public void SetState(State state) {
            this._State = state;
        }

        public void SetRouteColor(Color color) {
            _RouteColor = color;
        }
        
        public Color GetRouteColor() => _RouteColor;
    }

    [Serializable]
    public class RouteColor {
        public Color color;
        public State state;
        public string name;
    }
    public static BFS Singleton { get; private set; }

    List<Node> _graph = new List<Node>();
    
    [SerializeField] private Transform[] nodes;
    [SerializeField] private Transform transHub;
    [Space]
    [SerializeField] private GameObject connectorPrefab;
    [SerializeField] private float routeWidth = 0.2f;
    [Space]
    [SerializeField] private RouteColor[] routeColors;

    [SerializeField] private GameObject[] routes;
    
    private Dictionary<Node, List<Node>> possibleRoutes = new Dictionary<Node, List<Node>>();
    
    private Node _t;
    
    // Example Code
    Node _examplePoint;
    private bool exampleSet;
    [SerializeField] private bool _exampleToggle = false;
    
    List<Node> FindNearestPoints(List<Node> nodes, Node node) 
    {
        // nodes.Remove(node);
        
        // nodes is a pointer to _graph and removing the
        // point does not preserve original data.
        // to fix we're going to create a local copy
        List<Node> localNodes = new List<Node>(nodes);
        
        localNodes.Remove(node);
        
        List<Node> nearestNodes = new List<Node>();
        int minDistance = int.MaxValue;

        foreach (Node child in localNodes) {
            int distance = Mathf.RoundToInt(Vector3.Distance(child.GetPosition().position, node.GetPosition().position));
            
            if(distance < minDistance) nearestNodes = new List<Node>();
            
            if (distance <= minDistance) {
                minDistance = distance;
                nearestNodes.Add(child);
            }
        }

        return nearestNodes;
    }

    public void ActivateRoutes() {
        routes[0].SetActive(true);
    }

    void DrawConnections(Node node) {
        foreach (Node connectingNode in node.GetConnectedNodes()) {
            Debug.Log($"Drawing connection from {node.GetPosition().name} to {connectingNode.GetPosition().name}");
            Transform startPosition = node.GetPosition();
            Transform endPosition = connectingNode.GetPosition();

            Vector3 midPoint = (startPosition.position + endPosition.position) / 2;
            Vector3 direction = endPosition.position - startPosition.position;

            GameObject connector = Instantiate(connectorPrefab);
            connector.transform.position = midPoint;
            connector.transform.rotation = Quaternion.LookRotation(direction);
            connector.transform.localScale = new Vector3(routeWidth, routeWidth, direction.magnitude);
            
            connector.GetComponent<MeshRenderer>().material.SetColor("_Color", node.GetRouteColor());
        }
    }

    void ActivateNode(Node node, List<Node> previousNodes = null, Color previousRouteColor = default(Color)) {
        if(previousNodes == null) previousNodes = new List<Node>();
        
        if (previousRouteColor == default(Color)) 
            for (int i = 0; i < routeColors.Length; i++) {
                if (routeColors[i].state == State.Inactive) {
                    routeColors[i].state = State.Active;
                    previousRouteColor = routeColors[i].color;
                    break;
                }
            }

        List<Node> neighbors = possibleRoutes[node]; 
        if (neighbors.Count == 0) return;
        
        Node connectingNode = null;
        
        if (previousRouteColor != default(Color)) node.SetRouteColor(previousRouteColor);
        else {
            Debug.LogError("All route colors have been used");
        }

        if (neighbors.Contains(_t)) { // if we found the transportation hub we've finished 
            node.AddConnection(_t);
        
            DrawConnections(node);
            return;
        }

        foreach (Node neighbor in neighbors) {
            if (neighbor.GetState() == State.Active && !previousNodes.Contains(neighbor)) {
                connectingNode = neighbor;
                break;
            }
        }
        
        // if we get to this point that means there's no active neighbor
        foreach (Node neighbor in neighbors) {
            if (connectingNode != null) break;
            
            if (!previousNodes.Contains(neighbor)) {
                connectingNode = neighbor;
                break;
            }
        }

        if (connectingNode == null) {
            Debug.LogError("No connecting node found for graph on activation");
            return;
        }
        
        node.AddConnection(connectingNode);
        node.SetState(node.GetState() == State.Active ? State.Inactive : State.Active);
        previousNodes.Add(node);
        
        if(connectingNode.GetState() == State.Inactive) ActivateNode(connectingNode, previousNodes, previousRouteColor);
        
        DrawConnections(node);
    }

    void InitPossibleConnections() {
        Dictionary<Node, List<Node>> nearestNodes = new Dictionary<Node, List<Node>>();
        
        foreach (Node node in _graph) {
            nearestNodes.Add(node, FindNearestPoints(_graph, node));
        }

        foreach (Node node in nearestNodes.Keys) {
            for (int i = 0; i < nearestNodes[node].Count; i++) 
            {
                if(!nearestNodes[nearestNodes[node][i]].Contains(node)) 
                    nearestNodes[nearestNodes[node][i]].Add(node); // 
                    Debug.Log(node.GetPosition().name + ": " + nearestNodes[node][i].GetPosition().name);
            }
        }
        
        possibleRoutes = nearestNodes;
    }
    
    void Awake() {
        Debug.Log("BFS: Awake method called, initializing...");
        foreach (Transform node in nodes) _graph.Add(new Node(node.transform));
        InitPossibleConnections();
        _t = _graph.First(n => n.GetPosition().position == transHub.position);
        
        if (Singleton != null && Singleton != this) Destroy(this.gameObject);
        else Singleton = this;
    }


    //My Code:
    List<Node> blueRoute = new List<Node>();
    List<Node> redRoute = new List<Node>();

    void Start() 
    {
        Color node_route_color;
        
        if (_graph.Count > 0) {

            // To activate all nodes:
            // foreach (Node nodeGraph in _graph)
            print("!!!!! " + _graph.Count);
            _graph[0].AddConnection(_graph[1]);
            _graph[1].AddConnection(_graph[2]);
            _graph[2].AddConnection(_graph[3]);
            _graph[3].AddConnection(_graph[0]);

           _graph[4].AddConnection(_graph[5]);
           _graph[5].AddConnection(_graph[6]);
           _graph[6].AddConnection(_graph[7]);
           _graph[7].AddConnection(_graph[4]);
            
            _graph[0].SetRouteColor(Color.red);
            _graph[1].SetRouteColor(Color.red);
            _graph[2].SetRouteColor(Color.red);
            _graph[3].SetRouteColor(Color.red);

           _graph[4].SetRouteColor(Color.blue);
           _graph[5].SetRouteColor(Color.blue);
           _graph[6].SetRouteColor(Color.blue);
           _graph[7].SetRouteColor(Color.blue);

            DrawConnections(_graph[0]);
            DrawConnections(_graph[1]);
            DrawConnections(_graph[2]);
            DrawConnections(_graph[3]);

            DrawConnections(_graph[4]);
            DrawConnections(_graph[5]);
            DrawConnections(_graph[6]);
            DrawConnections(_graph[7]);

            //for (int i=0; i <_graph.Count; i++)
            //{
            //    if (i%2 == 0)
            //    {
            //        redRoute.Add(_graph[i]);
            //        node_route_color = routeColors[0].color;
            //        ActivateNode(_graph[i], redRoute, node_route_color);
            //        print("!!!");
            //    }
            //    else
            //    {
            //        blueRoute.Add(_graph[i]);
            //        node_route_color = routeColors[1].color;
            //        ActivateNode(_graph[i], blueRoute, node_route_color);
            //    }

            //    // ActivateNode(_graph[i], null, node_route_color);
            //}

            // to find all Route Colors:
            foreach (RouteColor routeColor in routeColors)
            {
                Debug.Log($"!Name: {routeColor.name}, Color: {routeColor.color}, State: {routeColor.state}");
            }

            Debug.Log("Activated node and drew connections.");
        } else {
            Debug.LogError("Graph is empty. Ensure nodes are assigned.");
        }
    }



}