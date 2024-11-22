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

    List<Node> _graph = new List<Node>();
    
    [SerializeField] private Transform[] nodes;
    [SerializeField] private Transform transHub;
    [Space]
    [SerializeField] private GameObject connectorPrefab;
    [SerializeField] private float routeWidth = 0.2f;
    [Space]
    [SerializeField] private RouteColor[] routeColors;
    
    private Dictionary<Node, List<Node>> possibleRoutes = new Dictionary<Node, List<Node>>();
    
    private Node _t;
    
    // Example Code
    Node _examplePoint;
    private bool exampleSet;
    [SerializeField] private bool _exampleToggle = false;
    
    List<Node> FindNearestPoints(List<Node> nodes, Node node) {
        // nodes.Remove(node);
        
        // nodes is a pointer to _graph and removing the start point does not preserve original data.
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

    void DrawConnections(Node node) {
        foreach (Node connectingNode in node.GetConnectedNodes()) {
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
                if(!nearestNodes[nearestNodes[node][i]].Contains(node)) 
                    nearestNodes[nearestNodes[node][i]].Add(node); // Debug.Log(node.GetPosition().name + ": " + nearestNodes[node][i].GetPosition().name);
        }
        
        possibleRoutes = nearestNodes;
    }

    void Awake() {
        foreach (Transform node in nodes) _graph.Add(new Node(node.transform));
        InitPossibleConnections();
        
        _examplePoint = _graph[4];
        _t = _graph.First(n => n.GetPosition().position == transHub.position);
        
        /*foreach (Node node in possibleRoutes.Keys) {
            for (int i = 0; i < possibleRoutes[node].Count; i++) Debug.Log(node.GetPosition().name + ": " + possibleRoutes[node][i].GetPosition().name);
        }*/
        
        // ActivateNode(_examplePoint);
    }

    void Update() {
        if (_exampleToggle && !exampleSet) {
            ActivateNode(_examplePoint);
            exampleSet = true;
        }
    }
}