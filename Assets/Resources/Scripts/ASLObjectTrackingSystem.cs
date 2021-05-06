using System;
using System.Collections.Generic;
using UnityEngine;
using ASL;


public class ObjectList<T> {
    public static int listCount = 0;
    string listName = null;

    private  List<T> objectList;
    public event Action<T> AddedEvent;
    public event Action<T> RemovedEvent;

    public ObjectList(string newListName = null) {
        listName = newListName;
        if (listName == null) {
            listName = "New List" + listCount.ToString("D3");
        }

        objectList = new List<T>();
    }

    public  bool AddToTrack(T toTrack) {
        // try to emplace
        if (objectList.Contains(toTrack)) {
            return false;
        } else {
            objectList.Add(toTrack);
            AddedEvent?.Invoke(toTrack);
            return true;
        }
    }

    public  bool RemoveToTrack(T toRemove) {
        // try to emplace
        if (objectList.Contains(toRemove)) {
            objectList.Remove(toRemove);
            RemovedEvent?.Invoke(toRemove);
            return true;
        } else {
            return false;
        }
    }

    public  List<T> GetList() {
        List<T> list = new List<T>();
        foreach (var obj in objectList) {
            list.Add(obj);
        }
        return list;
    }
}

public static class ASLObjectTrackingSystem {
    public static event Action<ASLObject> playerAddedEvent;
    public static event Action<ASLObject> playerRemovedEvent;
    public static event Action<ASLObject> objectAddedEvent;
    public static event Action<ASLObject> objectRemovedEvent;


    private static List<ASLObject> playersInScene = new List<ASLObject>();
    private static List<ASLObject> objectsInScene = new List<ASLObject>();

    private static ObjectList<ASLObject> playerList = new ObjectList<ASLObject>("PlayersInScene");
    public static ObjectList<ASLObject> PlayerList { get => playerList; set => playerList = value; }


    //==Player==
    public static bool AddPlayerToTrack(ASLObject playerToTrack) {
        // try to emplace
        if (playersInScene.Contains(playerToTrack)) {
            return false;
        } else {
            playersInScene.Add(playerToTrack);
            playerAddedEvent?.Invoke(playerToTrack);
            return true;
        }


    }


    public static bool RemovePlayerToTrack(ASLObject playerToRemove) {
        // try to emplace
        if (playersInScene.Contains(playerToRemove)) {
            playersInScene.Remove(playerToRemove);
            playerRemovedEvent?.Invoke(playerToRemove);
            return true;
        } else {
            return false;
        }
    }

    /// <summary>
    /// Deep copy of playersInScene and return a new list.
    /// </summary>
    /// <returns></returns>
    public static List<Transform> GetPlayers() {
        List<Transform> players = new List<Transform>();
        foreach (var obj in playersInScene) {
            players.Add(obj.transform);
        }
        return players;
    }

    //---Objects--------------------------------------------------
    public static bool AddObjectToTrack(ASLObject objectToTrack) {
        if (objectsInScene.Contains(objectToTrack)) {
            return false;
        } else {
            objectsInScene.Add(objectToTrack);
            objectAddedEvent?.Invoke(objectToTrack);
            return true;
        }
    }

    public static bool RemoveObjectToTrack(ASLObject objectToRemove)
    {
        if (objectsInScene.Contains(objectToRemove)) {
            objectsInScene.Remove(objectToRemove);
            objectRemovedEvent?.Invoke(objectToRemove);
            return true;
        } else {
            return false;
        }
    }
    public static List<Transform> GetObjects() {
        List<Transform> objects = new List<Transform>();
        /*
        foreach (var pair in ASLHelper.m_ASLObjects) {
            players.Add(pair.Value.transform);
        }
        */
        foreach (var obj in objectsInScene) {
            objects.Add(obj.transform);
        }
        return objects;
    }
}
