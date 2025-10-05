using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;

public class Decay : MonoBehaviour
{
    #region Variables
    [Header("Localized Decay Elements")]

    [Tooltip("This should be how long the decay takes to reach max in seconds")]
    [SerializeField] private float maxDecay = 30f; //default value of 100
    private float currDecay = 0;

    [Tooltip("Check this on creatures if they never decay")]
    [SerializeField] private bool isDecaying = false;

    [Tooltip("Should this enemy be destroyed upon max decay?")]
    [SerializeField] private bool destroyOnDecay = true;
    [Tooltip("How long till the destruction?")]
    [SerializeField] private float cooldownInterval = 5f;
    private float timer = 0;

    private bool isDestroying = false;
    private bool hasPossessed = false;

    private Image decayBar;

    private PossessionManager possessionManager;
    #endregion

    #region Getters and Setters
    public void SetDecaying(bool isDecaying)
    {
        this.isDecaying = isDecaying;
    }
    public bool GetDecaying()
    {
        return isDecaying;
    }
    public void SetPossessed(bool isPossessed)
    {
        hasPossessed = isPossessed;
    }
    public bool GetPossessed()
    {
        return hasPossessed;
    }

    public void SetIsDestroying(bool isDestroying)
    {
        this.isDestroying = isDestroying;
    }
    public bool GetIsDestroying()
    {
        return isDestroying;
    }

    public void ResetDestroyTimer()
    {
        timer = 0;
    }
    #endregion

    private void Start()
    {
        possessionManager = GetComponent<PossessionManager>();
        decayBar = GameObject.FindGameObjectWithTag("DecayBar").GetComponent<Image>();
    }

    private void Update()
    {
        DoDecay();
        UpdateDecayBar();
    }

    private void DoDecay()
    {
        if (isDecaying)
        {
            Debug.Log("Time till decay death: " + (maxDecay - currDecay));
            if (currDecay > maxDecay)
            {
                possessionManager.ExitHost();
                isDecaying = false;
                isDestroying = true;
                EmptyDecayBar();
            }
            currDecay += Time.deltaTime;
        }
        if (isDestroying)
        {
            if (destroyOnDecay && timer >= cooldownInterval)
            {
                Destroy(gameObject);
            }
            else
            {
                timer += Time.deltaTime;
            }
        }
    }

    private void UpdateDecayBar()
    {
        if (decayBar == null || !isDecaying) return; //if this object is not decaying it should not be updating
        float percentage = (currDecay / maxDecay);
        decayBar.fillAmount = percentage;
    }

    public void EmptyDecayBar()
    {
        if (decayBar == null)
        {
            Debug.Log("Decay Bar is Null");
            return;
        }
        decayBar.fillAmount = 0f;
    }

}
