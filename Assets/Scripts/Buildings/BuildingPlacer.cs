using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class BuildingPlacer : MonoBehaviour
{
    private Building _placedBuilding = null; 
    private Ray _ray;
    private RaycastHit _raycastHit;
    private Vector3 _lastPlacementPosition;
    GameManager gameManager;

    private void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        // instantiate headquarters at the beginning of the game
        _placedBuilding = new Building(gameManager.gameGlobalParameters.initialBuilding);
        _placedBuilding.SetPosition(gameManager.startPosition);
        // link the data into the manager
        _placedBuilding.Transform.GetComponent<BuildingManager>().Initialize(_placedBuilding);
        _PlaceBuilding();
        // make sure we have no building selected when the player starts to play
        _CancelPlacedBuilding();
    }

    void Update()
    {
        if (GameManager.instance.gameIsPaused) return;
        if (_placedBuilding != null)
        {
            if (Input.GetKeyUp(KeyCode.Escape))
            {
                _CancelPlacedBuilding();
                return;
            }
            _ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(_ray, out _raycastHit, 1000f, Globals.TERRAIN_LAYER_MASK))
            {
                _placedBuilding.SetPosition(_raycastHit.point);
                if (_lastPlacementPosition != _raycastHit.point)
                {
                    _placedBuilding.CheckValidPlacement();
                }
                _lastPlacementPosition = _raycastHit.point;
            }

            if (_placedBuilding.HasValidPlacement && Input.GetMouseButtonUp(0) && !EventSystem.current.IsPointerOverGameObject())
            {
                _PlaceBuilding();
                EventManager.TriggerEvent("PlaySoundByName", "onBuildingPlacedSound");
            }

        }
    }

    void _PreparePlacedBuilding(int buildingDataIndex)
    {
        // destroy the previous "phantom" if there is one
        if (_placedBuilding != null && !_placedBuilding.IsFixed)
        {
            Destroy(_placedBuilding.Transform.gameObject);
        }
        //gets building data from global index of available buildings
        Building building = new Building(Globals.BUILDING_DATA[buildingDataIndex]);
        // link the data into the manager
        building.Transform.GetComponent<BuildingManager>().Initialize(building);
        _placedBuilding = building;
        _lastPlacementPosition = Vector3.zero;

    }

    void _CancelPlacedBuilding()
    {
        // destroy the "phantom" building
        Destroy(_placedBuilding.Transform.gameObject);
        _placedBuilding = null;
    }
    void _PlaceBuilding()
    {
        _placedBuilding.Place();
        // keep on building the same building type if there are enough resources
        if (_placedBuilding.CanBuy())
        {
            _PreparePlacedBuilding(_placedBuilding.DataIndex);
        }
        else
        {
            EventManager.TriggerEvent("PlaceBuildingOff");
            _placedBuilding = null; 
        }

        EventManager.TriggerEvent("UpdateResourceTexts");
        EventManager.TriggerEvent("CheckBuildingButtons");

        // update the dynamic nav mesh
        Globals.UpdateNavMeshSurface();
    }
    public void SelectPlacedBuilding(int buildingDataIndex)
    {
        _PreparePlacedBuilding(buildingDataIndex);
    }
}
