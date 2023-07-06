using UnityEngine;
using Unity.AI.Navigation;

public class GameManager : MonoBehaviour
{

    public static GameManager instance;
    public Vector3 startPosition;

    public GameGlobalParameters gameGlobalParameters;

    private Ray _ray;
    private RaycastHit _raycastHit;
    [HideInInspector]
    public bool gameIsPaused;
    void Awake()
    {
        DataHandler.LoadGameData();
        GetComponent<DayAndNightCycler>().enabled = gameGlobalParameters.enableDayAndNightCycle;

        Globals.NAV_MESH_SURFACE = GameObject.Find("Terrain").GetComponent<NavMeshSurface>();
        Globals.UpdateNavMeshSurface();

        // enable/disable FOV depending on game parameters
        //GameObject.Find("FOV").SetActive(gameGlobalParameters.enableFOV);
        _GetStartPosition();
        gameIsPaused = false;
    }
    public void Start()
    {
        instance = this;
        // load all possible game parameters assets
        GameParameters[] gameParametersList =
          Resources.LoadAll<GameParameters>("ScriptableObjects/Parameters");
        // print the display name of each
        foreach (GameParameters parameters in gameParametersList)
        {
            Debug.Log(parameters.GetParametersName());
            Debug.Log("> Fields shown in-game:");
            foreach (string fieldName in parameters.FieldsToShowInGame)
                Debug.Log($"    {fieldName}");
        }
    }

    private void Update()
    {
        if (gameIsPaused) return;
        _CheckUnitsNavigation();
    }

    private void _GetStartPosition()
    {
        startPosition = Utils.MiddleOfScreenPointToWorld();
    }

    private void _CheckUnitsNavigation()
    {
        if (Globals.SELECTED_UNITS.Count > 0 && Input.GetMouseButtonUp(1))
        {
            _ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(
                _ray,
                out _raycastHit,
                1000f,
                Globals.TERRAIN_LAYER_MASK
            ))
            {
                foreach (UnitManager um in Globals.SELECTED_UNITS)
                    if (um.GetType() == typeof(CharacterManager))
                        ((CharacterManager)um).MoveTo(_raycastHit.point);
            }
        }
    }
    private void OnEnable()
    {
        EventManager.AddListener("PauseGame", _OnPauseGame);
        EventManager.AddListener("ResumeGame", _OnResumeGame);
    }

    private void OnDisable()
    {
        EventManager.RemoveListener("PauseGame", _OnPauseGame);
        EventManager.RemoveListener("ResumeGame", _OnResumeGame);
    }

    private void _OnPauseGame()
    {
        gameIsPaused = true;
        Time.timeScale = 0;
    }

    private void _OnResumeGame()
    {
        gameIsPaused = false;
        Time.timeScale = 1;
    }
}
