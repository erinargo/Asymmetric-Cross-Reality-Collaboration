using System;
using System.Collections;
using System.Collections.Generic;
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

        public Node(Transform position, State state = State.Inactive) {
            this._Position = position;
            this._State = state;
        }

        public State GetState() => _State;
        public Transform GetPosition() => _Position;

        public void SetState(State state) {
            this._State = state;
        }
    }
    
    List<Node> _graph = new List<Node>();
    
    [SerializeField] private Transform[] nodes;
    [SerializeField] private Transform transHub;
    [Space]
    [SerializeField] private GameObject connectorPrefab;
    [SerializeField] private float routeWidth = 0.2f;
    
    private Node _t;
    
    // Example Code
    Node _examplePoint;
    
    List<Node> FindNearestPoints(List<Node> nodes, Node node) {
        nodes.Remove(node);
        
        List<Node> nearestNodes = new List<Node>();
        int minDistance = int.MaxValue;

        foreach (Node child in nodes) {
            int distance = Mathf.RoundToInt(Vector3.Distance(child.GetPosition().position, node.GetPosition().position));
            
            if(distance < minDistance) nearestNodes = new List<Node>();
            
            if (distance <= minDistance) {
                minDistance = distance;
                nearestNodes.Add(child);
            }
        }

        return nearestNodes;
    }

    void Awake() {
        foreach (Transform node in nodes) _graph.Add(new Node(node.transform));
        
        _examplePoint = _graph[4];
        _t = new Node(transHub, State.Active);
    }

    void ConnectNode(Node start, Node end) {
        // Calculate direction vector from start node to end node
        Vector3 direction = (end.GetPosition().position - start.GetPosition().position).normalized;

        if (direction.x < 1 && direction.z < 1) return;
        
        GameObject connector = Instantiate(connectorPrefab);
        connector.transform.position = start.GetPosition().position;
        
        Debug.Log(start.GetPosition().name);
        Debug.Log(start.GetPosition().position);
        Debug.Log(end.GetPosition().name);
        Debug.Log(end.GetPosition().position);
        
        connector.transform.localScale = direction.x < 1 ? new Vector3((direction.z * 2), 0, routeWidth) : new Vector3(routeWidth, 0, (direction.x * 2)); 
    }

    void ActivateNode(Node node, bool playerChosen = false) {
        if(playerChosen) node.SetState(node.GetState() == State.Active ? State.Inactive : State.Active);
        
        List<Node> neighbors = FindNearestPoints(_graph, node);
        if (neighbors.Count == 0) return;
        
        List<Node> activeNeighbours = neighbors.FindAll(n => n.GetState() == State.Active);
        Node connectingNode = activeNeighbours.Count < 1 ? neighbors[0] : activeNeighbours[0];

        if (activeNeighbours.Contains(_t)) connectingNode = _t;
        ConnectNode(node, connectingNode);
        
        Debug.Log(connectingNode.GetPosition().name);
        
        // connectingNode == _t ? ActivateNode(connectingNode) : false;
        ActivateNode(connectingNode);
    }

    void Start() {
        
        Debug.Log(_examplePoint.GetPosition().name);
        ActivateNode(_examplePoint);
    }
}
