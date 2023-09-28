using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;


[RequireComponent(typeof(ARTrackedImageManager))]
public class PlaceTrackedImages : MonoBehaviour {

    
    private ARTrackedImageManager _trackedImagesManager;

    //^ List of prefabs
    //^ Should be named as their corresponding 2D image in the ReferenceImageLibrary
    public GameObject[] ARPrefabs;

    private readonly Dictionary<string, GameObject> _instantiatedPrefabs = new Dictionary<string, GameObject>();

   
    private void Awake() {
        _trackedImagesManager = GetComponent<ARTrackedImageManager>();
    }

    private void OnEnable() {
        _trackedImagesManager.trackedImagesChanged += _trackedImagesManager_trackedImagesChanged;
    }

    private void OnDisable() {
        _trackedImagesManager.trackedImagesChanged -= _trackedImagesManager_trackedImagesChanged;
    }

    private void _trackedImagesManager_trackedImagesChanged(ARTrackedImagesChangedEventArgs objEventArgs) {

        //^ #1
        //^ Loop through all new detected images
        foreach(var trackedImage in objEventArgs.added) {

            var imageName = trackedImage.referenceImage.name;

            //^ Search for mathches prefab for image
            foreach (var currentPrefab in ARPrefabs) {

                //^ If prefab name == image name
                //^ & the prefab hasn't already been created
                if (string.Compare(currentPrefab.name, imageName, StringComparison.OrdinalIgnoreCase) == 0 
                    && !_instantiatedPrefabs.ContainsKey(imageName)) {

                    var newPrefab = Instantiate(currentPrefab, trackedImage.transform);
                    _instantiatedPrefabs[imageName] = newPrefab;
                }
            }
        }

        //^ #2
        //^ Turn off all instantiated objects, that left the viewfield
        //^ & Turn on, when they enter again
        foreach (var trackedImage in objEventArgs.updated) {
            _instantiatedPrefabs[trackedImage.referenceImage.name].SetActive(trackedImage.trackingState == TrackingState.Tracking);
        }

        //^ #3
        //^ When AR subsystem has given up looking for image
        foreach(var trackedImage in objEventArgs.removed) {
            Destroy(_instantiatedPrefabs[(trackedImage.referenceImage.name)]);
            _instantiatedPrefabs.Remove(trackedImage.referenceImage.name);
        }
    }
}