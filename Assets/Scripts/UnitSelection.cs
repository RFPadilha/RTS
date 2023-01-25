using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitSelection : MonoBehaviour
{
    private bool _isDraggingMouseBox = false;
    private Vector3 _dragStartPosition;
    Ray _ray;
    RaycastHit _raycastHit;
    public UIManager uiManager;
    private KeyCode[] keyCodes = new KeyCode[] { //numeric keycodes for control groups
        KeyCode.Alpha0, 
        KeyCode.Alpha1, 
        KeyCode.Alpha2, 
        KeyCode.Alpha3, 
        KeyCode.Alpha4, 
        KeyCode.Alpha5, 
        KeyCode.Alpha6, 
        KeyCode.Alpha7, 
        KeyCode.Alpha8, 
        KeyCode.Alpha9 };

    //used for army hotkeys (selection grouping), associates an int to a list, so pressing a numeric key will select a list of units
    private Dictionary<int, List<UnitManager>> _selectionGroups = new Dictionary<int, List<UnitManager>>();

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            _isDraggingMouseBox = true;
            _dragStartPosition = Input.mousePosition;
        }

        if (Input.GetMouseButtonUp(0))
            _isDraggingMouseBox = false;

        if (_isDraggingMouseBox && _dragStartPosition != Input.mousePosition)
            _SelectUnitsInDraggingBox();

        if (Globals.SELECTED_UNITS.Count > 0)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
                _DeselectAllUnits();
            if (Input.GetMouseButtonDown(0))
            {
                _ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(
                    _ray,
                    out _raycastHit,
                    1000f
                ))
                {
                    if (_raycastHit.transform.CompareTag("Terrain"))
                        _DeselectAllUnits();
                }
            }
        }

        // manage selection groups with alphanumeric keys
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift) ||//REPLACE FOR WINDOWS CONTROL KEYS FOR BUILD
                    Input.GetKey(KeyCode.LeftApple) || Input.GetKey(KeyCode.RightApple))//mac "cmd" keys
        {
            for (int i = 0; i < keyCodes.Length; ++i)
            {
                if (Input.GetKeyDown(keyCodes[i]))
                {
                    Debug.Log("replace shift for control before build");
                    _CreateSelectionGroup(i);
                }
            }
        }
        if (Input.anyKeyDown)//whenever a keyboard key is pressed
        {
            int alphaKey = Utils.GetAlphaKeyValue(Input.inputString);
            if (alphaKey != -1)//verifies if its a number between 1-9, commonly used for army hotkeys
            {
                Debug.Log("Attempted to select selection group");
                _ReselectGroup(alphaKey);
            }

        }
    }
    private void _DeselectAllUnits()
    {
        List<UnitManager> selectedUnits = new List<UnitManager>(Globals.SELECTED_UNITS);
        foreach (UnitManager um in selectedUnits)
            um.Deselect();
    }
    private void _SelectUnitsInDraggingBox()
    {
        Bounds selectionBounds = Utils.GetViewportBounds(
            Camera.main,
            _dragStartPosition,
            Input.mousePosition
        );
        GameObject[] selectableUnits = GameObject.FindGameObjectsWithTag("Unit");
        bool inBounds;
        foreach (GameObject unit in selectableUnits)
        {
            inBounds = selectionBounds.Contains(
                Camera.main.WorldToViewportPoint(unit.transform.position)
            );
            if (inBounds)
                unit.GetComponent<UnitManager>().Select();
            else
                unit.GetComponent<UnitManager>().Deselect();
        }
    }


    //Army grouping/hotkeys-------------------------------------------------------------------------------------
    public void SelectUnitsGroup(int groupIndex)
    {
        _ReselectGroup(groupIndex);
    }

    private void _CreateSelectionGroup(int groupIndex)
    {
        // check there are units currently selected
        if (Globals.SELECTED_UNITS.Count == 0)
        {
            if (_selectionGroups.ContainsKey(groupIndex))
                _RemoveSelectionGroup(groupIndex);//no units selected deletes control group
            return;
        }
        List<UnitManager> groupUnits = new List<UnitManager>(Globals.SELECTED_UNITS);
        _selectionGroups[groupIndex] = groupUnits;
        uiManager.ToggleSelectionGroupButton(groupIndex, true);
    }

    private void _RemoveSelectionGroup(int groupIndex)
    {
        _selectionGroups.Remove(groupIndex);
        _selectionGroups.Remove(groupIndex);
        uiManager.ToggleSelectionGroupButton(groupIndex, false);
    }

    private void _ReselectGroup(int groupIndex)
    {
        // check the group actually is defined
        if (!_selectionGroups.ContainsKey(groupIndex)) return;
        //deselects everything if not defined
        _DeselectAllUnits();
        foreach (UnitManager um in _selectionGroups[groupIndex])
            um.Select();
    }


    //auxiliaries------------------------------------------------------------------------------------------
    void OnGUI()
    {
        if (_isDraggingMouseBox)
        {
            // Create a rect from both mouse positions
            var rect = Utils.GetScreenRect(_dragStartPosition, Input.mousePosition);
            Utils.DrawScreenRect(rect, new Color(0.5f, 1f, 0.4f, 0.2f));
            Utils.DrawScreenRectBorder(rect, 1, new Color(0.5f, 1f, 0.4f));
        }
    }
}
