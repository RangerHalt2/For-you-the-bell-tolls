//Purpose: To handle all inputs in a dynamic system that can be updated in future iterations of the game and add new inputs easily.
//Contributor and Author: Logan Baysinger. 

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    #region Variables
    [SerializeField] public InputActionAsset playerControls;

    [SerializeField] public string actionMapName = "Player";//This should be the name of the entire mapping

    //LB: These should be the names of individual input readings for the broad inputs like WASD is part of movement and jumping assigns spacebar and A/Circle on controllers
    [SerializeField] private string movement = "Move";
    [SerializeField] private string jump = "Jump";
    [SerializeField] private string dash = "Dash";
    [SerializeField] private string possess = "Possess";
    [SerializeField] private string exitpossession = "ExitPossession";
    [SerializeField] private string interact = "Interact";
    [SerializeField] private string attack = "Attack";

    //LB: This is an action input, each one needs one assigned
    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction dashAction;
    private InputAction possessAction;
    private InputAction exitAction;
    public InputAction interactAction;
    private InputAction attackAction;
    #endregion

    #region Getters/Setters
    //LB: This is the getters and setters for the inputs, this will be used to manage their values overall
    public Vector2 MoveInput { get; private set; }
    public bool JumpInput { get; private set; }
    public bool DashInput { get; private set; }
    public bool PossessInput { get; private set; }
    public bool ExitInput {  get; private set; }
    public bool InteractInput { get; private set; }
    public bool AttackInput { get; private set; }
    #endregion

    #region Input Action Context and Values
    //LB: Instance Handler
    public static InputManager Instance { get; private set; }

    void Awake()
    {
        //LB: Handles instance related stuff, still unsure where and when the destroy code runs? It never seems necessary
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            //Destroy(gameObject);
        }
        
        //LB: This assigns the values of the actions to the input actions from the physical asset
        moveAction = playerControls.FindActionMap(actionMapName).FindAction(movement);
        jumpAction = playerControls.FindActionMap(actionMapName).FindAction(jump);
        dashAction = playerControls.FindActionMap(actionMapName).FindAction(dash);
        possessAction = playerControls.FindActionMap(actionMapName).FindAction(possess);
        exitAction = playerControls.FindActionMap(actionMapName).FindAction(exitpossession);
        interactAction = playerControls.FindActionMap(actionMapName).FindAction(interact);
        attackAction = playerControls.FindActionMap(actionMapName).FindAction(attack);
        RegisterInputActions();
    }

    //LB: This piece of code handles their values with pseudo functions
    void RegisterInputActions()
    {
        moveAction.performed += context => MoveInput = context.ReadValue<Vector2>();
        moveAction.canceled += context => MoveInput = Vector2.zero;

        jumpAction.performed += context => JumpInput = true;
        jumpAction.canceled += context => JumpInput = false;

        dashAction.performed += context => DashInput = true;
        dashAction.canceled += context => DashInput = false;

        possessAction.performed += context => PossessInput = true;
        possessAction.canceled += context => PossessInput = false;

        exitAction.performed += context => ExitInput = true;
        exitAction.canceled += context => ExitInput = false;

        interactAction.performed += context => InteractInput = true;
        interactAction.canceled += context => InteractInput = false;

        attackAction.performed += context => AttackInput = true;
        attackAction.canceled += context => AttackInput = false;
    }
    #endregion

    #region Enable/Disable
    //LB: Enable and Disable the actions
    private void OnEnable()
    {
        moveAction.Enable();
        jumpAction.Enable();
        dashAction.Enable();
        possessAction.Enable();
        exitAction.Enable();
        interactAction.Enable();
        attackAction.Enable();
    }

    private void OnDisable()
    {
        moveAction.Disable();
        jumpAction.Disable();
        dashAction.Disable();
        possessAction.Disable();
        exitAction.Disable();
        interactAction.Disable();
        attackAction.Disable();
    }
    #endregion
}
