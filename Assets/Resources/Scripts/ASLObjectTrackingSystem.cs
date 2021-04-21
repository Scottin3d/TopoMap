using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ASL;

public static class ASLObjectTrackingSystem { 

    private static Dictionary<ASLObject, Transform> ASLObjectsInScene = new Dictionary<ASLObject, Transform>();
    private static Dictionary<ASLObject, Transform> ASLPlayersInScene = new Dictionary<ASLObject, Transform>();



    private static int numberOfPlayers = 0;
    public static int NumberOfPlayers { get => numberOfPlayers; set => numberOfPlayers = value; }
    public static int NumberOfObjects { get => numberOfObjects; set => numberOfObjects = value; }

    private static int numberOfObjects = 0;
    public static bool AddPlayerToTrack(ASLObject playerToTrack, Transform playerTransform) {
        // try to emplace
        if (ASLPlayersInScene.ContainsKey(playerToTrack)) {
            return false;

            // add if not
        } else {
            ASLPlayersInScene.Add(playerToTrack, playerTransform);
            numberOfPlayers++;
            return true;
        }
    }

    public static bool RemovePlayerToTrack(ASLObject playerToRemove) {
        // try to emplace
        if (ASLPlayersInScene.ContainsKey(playerToRemove)) {
            ASLPlayersInScene.Remove(playerToRemove);
            numberOfPlayers = (numberOfPlayers - 1 >= 0) ? numberOfPlayers-- : 0;
            return true;

            // add if not
        } else {
            return false;
        }
    }
    public static List<Transform> GetPlayers() {
        List<Transform> players = new List<Transform>();
        foreach (var pair in ASLPlayersInScene) {
            players.Add(pair.Value);
        }
        return players;
    }

    public static Transform GetPlayerTransform(ASLObject playerToTrack) {
        if (ASLPlayersInScene.ContainsKey(playerToTrack)) {
            return ASLPlayersInScene[playerToTrack];
        } else {
            return null;
        }

    }

    //---Objects--------------------------------------------------

    public static void UpdatePlayerTransform(ASLObject playerToTrack, Transform playerTransform) {
        // try to emplace
        if (ASLPlayersInScene.ContainsKey(playerToTrack)) {
            ASLPlayersInScene[playerToTrack] = playerTransform;
            return;

            // add if not
        } else {
            AddPlayerToTrack(playerToTrack, playerTransform);
            return;
        }
    }

    public static bool AddObjectToTrack(ASLObject objectToTrack, Transform objectTransform) {
        // try to emplace
        if (ASLObjectsInScene.ContainsKey(objectToTrack)) {
            return false;

        // add if not
        } else {
            ASLObjectsInScene.Add(objectToTrack, objectTransform);
            return true;
        }
    }

    public static void UpdateObjectTransform(ASLObject objectToTrack, Transform objectTransform) {
        // try to emplace
        if (ASLObjectsInScene.ContainsKey(objectToTrack)) {
            ASLObjectsInScene[objectToTrack] = objectTransform;
            return;

            // add if not
        } else {
            AddObjectToTrack(objectToTrack, objectTransform);
            return;
        }
    }

    // object
    public static Transform GetObjectTransform(ASLObject objectToTrack) {
        if (ASLObjectsInScene.ContainsKey(objectToTrack)) {
            return ASLObjectsInScene[objectToTrack];
        } else {
            return null;
        }
        
    }
    public static List<Transform> GetObjects() {
        List<Transform> objects = new List<Transform>();
        /*
        foreach (var pair in ASLHelper.m_ASLObjects) {
            players.Add(pair.Value.transform);
        }
        */
        foreach (var pair in ASLObjectsInScene) {
            objects.Add(pair.Value);
        }
        return objects;
    }


}
