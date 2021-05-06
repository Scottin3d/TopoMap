using System;
using System.Collections.Generic;
using UnityEngine;
using ASL;


/// <summary>
/// Generic object list class that has expanded functionality for adding and removing.
/// </summary>
/// <typeparam name="T">The list type</typeparam>
public class ObjectList<T> : List<T>{
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

/// <summary>
/// A more robust way to track network objects within the scene.
/// In the process of creating a generic version for better implementation.
/// </summary>
public static class ASLObjectTrackingSystem {
    public static event Action<ASLObject> playerAddedEvent;
    public static event Action<ASLObject> playerRemovedEvent;
    public static event Action<ASLObject> objectAddedEvent;
    public static event Action<ASLObject> objectRemovedEvent;


    private static List<ASLObject> playersInScene = new List<ASLObject>();
    private static List<ASLObject> objectsInScene = new List<ASLObject>();

    // generic testing
    /*
    private static ObjectList<ASLObject> playerList = new ObjectList<ASLObject>("PlayersInScene");
    public static ObjectList<ASLObject> PlayerList { get => playerList; set => playerList = value; }
    */

    //==Player List==
    /// <summary>
    /// Add a player ASLObject to track.
    /// </summary>
    /// <param name="playerToTrack">The ASLObject of the player to be tracked.</param>
    /// <returns>Bool if added or not.</returns>
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

    /// <summary>
    /// Remove a player ASLObject from being tracked.
    /// </summary>
    /// <param name="playerToRemove">The ASLObject of the player to be removed.</param>
    /// <returns>Bool if removed or not.</returns>
    public static bool RemovePlayerToTrack(ASLObject playerToRemove) {
        // try to remove
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
    /// <returns>A list of players tracked in the scene.</returns>
    public static List<Transform> GetPlayers() {
        List<Transform> players = new List<Transform>();
        foreach (var obj in playersInScene) {
            players.Add(obj.transform);
        }
        return players;
    }

    //==Object List==
    /// <summary>
    /// Add an object ASLObject to be track.
    /// </summary>
    /// <param name="objectToTrack">The ASLObject of the object to be tracked.</param>
    /// <returns>Bool if the object was added.</returns>
    public static bool AddObjectToTrack(ASLObject objectToTrack) {
        if (objectsInScene.Contains(objectToTrack)) {
            return false;
        } else {
            objectsInScene.Add(objectToTrack);
            objectAddedEvent?.Invoke(objectToTrack);
            return true;
        }
    }

    /// <summary>
    /// Remove an object ASLObject from being tracked.
    /// </summary>
    /// <param name="objectToRemove">The ASLObject of the object to be removed.</param>
    /// <returns>Bool if object removed.</returns>
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

    /// <summary>
    /// Get a list of objects being tracked in scene.
    /// </summary>
    /// <returns>A deep copy of the objects tracked in the scene.</returns>
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
