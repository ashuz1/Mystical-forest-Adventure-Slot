using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using System.Linq;
using TMPro;
using System;
using System.Reflection;

public class SlotBehaviour : MonoBehaviour
{
    [Header("Sprites")]
    [SerializeField]
    private Sprite[] myImages;  //images taken initially

    [Header("Slot Images")]
    [SerializeField]
    private List<SlotImage> images;     //class to store total images
    [SerializeField]
    private List<SlotREsultImage> Tempimages;     //class to store the result DummyReel

    [Header("Slots Transforms")]
    [SerializeField]
    private Transform[] Slot_Transform;


    [Header("Buttons")]
    [SerializeField]
    private Button SlotStart_Button;
    [SerializeField]
    private Button TBetPlus_Button;
    [SerializeField]
    private Button TBetMinus_Button;
    [SerializeField] private Button Turbo_Button;
    [SerializeField] private Button StopSpin_Button;

    [Header("Miscellaneous UI")]
    [SerializeField]
    private TMP_Text Balance_text;
    [SerializeField]
    private TMP_Text TotalBet_text;
    [SerializeField]
    private TMP_Text LineBet_text;
    [SerializeField]
    private TMP_Text TotalWin_text;

    [Header("Audio Management")]
    [SerializeField]
    private AudioController audioController;

    [SerializeField]
    private UIManager uiManager;


    [Header("Free Spins Board")]
    [SerializeField]
    private GameObject FSBoard_Object;
    [SerializeField]
    private TMP_Text FSnum_text;

    int tweenHeight = 0;  //calculate the height at which tweening is done

    [SerializeField]
    private GameObject Image_Prefab;    //icons prefab


    private List<Tweener> alltweens = new List<Tweener>();

    private Tweener WinTween = null;

    [SerializeField]
    private List<ImageAnimation> TempList;  //stores the sprites whose animation is running at present 

    [SerializeField]
    private RandomeResultGenrator randomeResult;

    private Coroutine AutoSpinRoutine = null;
    private Coroutine FreeSpinRoutine = null;
    private Coroutine tweenroutine;
    private Tween BalanceTween;
    internal bool IsAutoSpin = false;
    internal bool IsFreeSpin = false;
    private bool IsSpinning = false;
    private bool CheckSpinAudio = false;
    internal bool CheckPopups = false;
    internal int BetCounter = 0;
    private double currentBalance = 0;
    private double currentTotalBet = 0;
    protected int Lines = 20;
    [SerializeField]
    private int IconSizeFactor = 100;       //set this parameter according to the size of the icon and spacing
    private int numberOfSlots = 5;          //number of columns
    private bool StopSpinToggle;
    private float SpinDelay = 0.2f;
    private bool IsTurboOn;
    internal bool WasAutoSpinOn;

    private void Start()
    {
        IsAutoSpin = false;

        if (SlotStart_Button) SlotStart_Button.onClick.RemoveAllListeners();
        if (SlotStart_Button) SlotStart_Button.onClick.AddListener(delegate { StartSlots(); });

        if (TBetPlus_Button) TBetPlus_Button.onClick.RemoveAllListeners();
        if (TBetPlus_Button) TBetPlus_Button.onClick.AddListener(delegate { ChangeBet(true); });

        if (TBetMinus_Button) TBetMinus_Button.onClick.RemoveAllListeners();
        if (TBetMinus_Button) TBetMinus_Button.onClick.AddListener(delegate { ChangeBet(false); });

        if (StopSpin_Button) StopSpin_Button.onClick.RemoveAllListeners();
        if (StopSpin_Button) StopSpin_Button.onClick.AddListener(() => { audioController.PlayButtonAudio(); StopSpinToggle = true; StopSpin_Button.gameObject.SetActive(false); });


        if (FSBoard_Object) FSBoard_Object.SetActive(false);

        tweenHeight = (15 * IconSizeFactor) - 280;

        SetInitialUI();
        shuffleInitialDummyReel();
    }


 

    #region FreeSpin
    internal void FreeSpin(int spins)
    {
        if (!IsFreeSpin)
        {
            if (FSnum_text) FSnum_text.text = spins.ToString();
            if (FSBoard_Object) FSBoard_Object.SetActive(true);
            IsFreeSpin = true;
            ToggleButtonGrp(false);

            if (FreeSpinRoutine != null)
            {
                StopCoroutine(FreeSpinRoutine);
                FreeSpinRoutine = null;
            }
            FreeSpinRoutine = StartCoroutine(FreeSpinCoroutine(spins));
        }
    }

    private IEnumerator FreeSpinCoroutine(int spinchances)
    {
        int i = 0;
        while (i < spinchances)
        {
            uiManager.FreeSpins--;
            if (FSnum_text) FSnum_text.text = uiManager.FreeSpins.ToString();
            StartSlots();
            yield return tweenroutine;
            yield return new WaitForSeconds(SpinDelay);
            i++;
        }
        if (FSBoard_Object) FSBoard_Object.SetActive(false);
       
        
            ToggleButtonGrp(true);
        
        IsFreeSpin = false;
    }
    #endregion

    private void CompareBalance()
    {
        if (currentBalance < currentTotalBet)
        {
            uiManager.LowBalPopup();
        }
    }

    private void ChangeBet(bool IncDec)
    {
        if (audioController) audioController.PlayButtonAudio();
        if (IncDec)
        {
            BetCounter++;
            if (BetCounter >= randomeResult.bets.Count)
            {
                BetCounter = 0; // Loop back to the first bet
            }
        }
        else
        {
            BetCounter--;
            if (BetCounter < 0)
            {
                BetCounter = randomeResult.bets.Count - 1; // Loop to the last bet
            }
        }
        if (LineBet_text) LineBet_text.text = randomeResult.bets[BetCounter].ToString();
        if (TotalBet_text) TotalBet_text.text = (randomeResult.bets[BetCounter] * Lines).ToString();
        currentTotalBet = randomeResult.bets[BetCounter] * Lines;

    }

    #region InitialFunctions
    internal void shuffleInitialDummyReel()
    {
        for (int i = 0; i < Tempimages.Count; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                int randomIndex = UnityEngine.Random.Range(0, 12);
                Tempimages[i].slotImages[j].SetSymbol(randomIndex);
            }
        }
    }

    internal void SetInitialUI()
    {
        BetCounter = 0;

        if (TotalBet_text) TotalBet_text.text = (randomeResult.bets[BetCounter] * Lines).ToString();
        if (TotalWin_text) TotalWin_text.text = "0.000";
        if (Balance_text) Balance_text.text = randomeResult.balance.ToString("F3");

        currentBalance = randomeResult.balance;
        currentTotalBet = randomeResult.bets[BetCounter] * Lines;
        CompareBalance();
    }
    #endregion


    #region SlotSpin
    //starts the spin process
    private void StartSlots(bool autoSpin = false)
    {
        if (audioController) audioController.PlaySpinButtonAudio();

        if (!autoSpin)
        {
            if (AutoSpinRoutine != null)
            {
                StopCoroutine(AutoSpinRoutine);
                StopCoroutine(tweenroutine);
                tweenroutine = null;
                AutoSpinRoutine = null;
            }
        }
        WinningsAnim(false);
        if (SlotStart_Button) SlotStart_Button.interactable = false;
        if (TempList.Count > 0)
        {
            StopGameAnimation();
        }
        tweenroutine = StartCoroutine(TweenRoutine());
    }

    //manage the Routine for spinning of the slots
    private IEnumerator TweenRoutine()
    {
        if (currentBalance < currentTotalBet && !IsFreeSpin)
        {
            CompareBalance();
            yield return new WaitForSeconds(1);
            ToggleButtonGrp(true);
            yield break;
        }
        if (audioController) audioController.PlayWLAudio("spin");
        CheckSpinAudio = true;

        IsSpinning = true;

        ToggleButtonGrp(false);
        if (!IsTurboOn && !IsFreeSpin && !IsAutoSpin)
        {
            StopSpin_Button.gameObject.SetActive(true);
        }
        for (int i = 0; i < numberOfSlots; i++)
        {
            InitializeTweening(Slot_Transform[i]);
            yield return new WaitForSeconds(0.1f);
        }

        if (!IsFreeSpin)
        {
            BalanceDeduction();
        }                                                                    // REsult

        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 5; j++)
            {
                int resultNum = randomeResult.DummyReel[i].symbol[j];

                Tempimages[j].slotImages[i].GetComponent<ImageAnimation>().SetSymbol(resultNum);

            }
        }


        if (IsTurboOn)
        {

            yield return new WaitForSeconds(0.1f);
            StopSpinToggle = true;
        }
        else
        {
            for (int i = 0; i < 5; i++)
            {
                yield return new WaitForSeconds(0.1f);
                if (StopSpinToggle)
                {
                    break;
                }
            }
            StopSpin_Button.gameObject.SetActive(false);
        }

        for (int i = 0; i < numberOfSlots; i++)
        {
            yield return StopTweening(5, Slot_Transform[i], i, StopSpinToggle);
        }
        StopSpinToggle = false;

        yield return alltweens[^1].WaitForCompletion();
        KillAllTweens();

        if (randomeResult.winAmount > 0)
        {
            SpinDelay = 1.2f;
        }
        else
        {
            SpinDelay = 0.2f;
        }

        if (randomeResult.winAmount > 0)
        {
            List<int> winLine = new();
            foreach (var item in randomeResult.lines)
            {
                winLine.Add(item);
            }
            CheckPayoutLineBackend(winLine);
        }

        CheckPopups = true;

        if (TotalWin_text) TotalWin_text.text = randomeResult.winAmount.ToString("F3");
        BalanceTween?.Kill();
        if (Balance_text) Balance_text.text = randomeResult.balance.ToString("F3");

        currentBalance = randomeResult.balance;


        if (!IsAutoSpin && !IsFreeSpin)
        {
            ToggleButtonGrp(true);
            IsSpinning = false;
        }
        else
        {
            IsSpinning = false;
        }
       
    }

    private void BalanceDeduction()
    {
        double bet = 0;
        double balance = 0;
        try
        {
            bet = double.Parse(TotalBet_text.text);
        }
        catch (Exception e)
        {
            Debug.Log("Error while conversion " + e.Message);
        }

        try
        {
            balance = double.Parse(Balance_text.text);
        }
        catch (Exception e)
        {
            Debug.Log("Error while conversion " + e.Message);
        }
        double initAmount = balance;

        balance = balance - bet;

        BalanceTween = DOTween.To(() => initAmount, (val) => initAmount = val, balance, 0.8f).OnUpdate(() =>
        {
            if (Balance_text) Balance_text.text = initAmount.ToString("F3");
        });
    }



    //generate the payout lines generated 
    private void CheckPayoutLineBackend(List<int> LineId, double jackpot = 0)
    {

        if (LineId.Count > 0)
        {

            if (jackpot > 0)
            {
                if (audioController) audioController.PlayWLAudio("megaWin");
                for (int i = 0; i < Tempimages.Count; i++)
                {
                    for (int k = 0; k < Tempimages[i].slotImages.Count; k++)
                    {
                        StartGameAnimation(Tempimages[i].slotImages[k].gameObject);
                    }
                }
            }
            else
            {
                List<KeyValuePair<int, int>> coords = new();
                for (int j = 0; j < LineId.Count; j++)
                {
                    for (int k = 0; k < randomeResult.SymbolstoEmit[j].symbol.Count; k++)
                    {
                        int rowIndex = randomeResult.payline[LineId[j]].symbol[k];
                        int columnIndex = k;
                        coords.Add(new KeyValuePair<int, int>(rowIndex, columnIndex));
                    }
                }

                foreach (var coord in coords)
                {
                    int rowIndex = coord.Key;
                    int columnIndex = coord.Value;
                    StartGameAnimation(Tempimages[columnIndex].slotImages[rowIndex].gameObject);
                }
            }
            WinningsAnim(true);
        }
        else
        {

            //if (audioController) audioController.PlayWLAudio("lose");
            if (audioController) audioController.StopWLAaudio();
        }
        CheckSpinAudio = false;
    }

    private void WinningsAnim(bool IsStart)
    {
        if (IsStart)
        {
            WinTween = TotalWin_text.gameObject.GetComponent<RectTransform>().DOScale(new Vector2(1.5f, 1.5f), 1f).SetLoops(-1, LoopType.Yoyo).SetDelay(0);
        }
        else
        {
            WinTween.Kill();
            TotalWin_text.gameObject.GetComponent<RectTransform>().localScale = Vector3.one;
        }
    }

    #endregion


    void ToggleButtonGrp(bool toggle)
    {
        if (SlotStart_Button) SlotStart_Button.interactable = toggle;       
        if (TBetMinus_Button) TBetMinus_Button.interactable = toggle;
        if (TBetPlus_Button) TBetPlus_Button.interactable = toggle;
    }


    private void StartGameAnimation(GameObject animObjects)
    {
        ImageAnimation temp = animObjects.GetComponent<ImageAnimation>();
        temp.PlayWinAnimation();
        TempList.Add(temp);
    }

    //stop the icons animation
    private void StopGameAnimation()
    {
        for (int i = 0; i < TempList.Count; i++)
        {
            TempList[i].SetSymbol(0, true);
        }
        TempList.Clear();
        TempList.TrimExcess();
    }


    #region TweeningCode
    private void InitializeTweening(Transform slotTransform)
    {
        slotTransform.localPosition = new Vector2(slotTransform.localPosition.x, 0);
        Tweener tweener = slotTransform.DOLocalMoveY(-tweenHeight, 0.2f).SetLoops(-1, LoopType.Restart).SetDelay(0);
        tweener.Play();
        alltweens.Add(tweener);
    }



    private IEnumerator StopTweening(int reqpos, Transform slotTransform, int index, bool isStop)
    {
        alltweens[index].Kill();
        int tweenpos = (reqpos * IconSizeFactor) - IconSizeFactor;
        slotTransform.localPosition = new Vector2(slotTransform.localPosition.x, 0);
        alltweens[index] = slotTransform.DOLocalMoveY(112, 0.5f).SetEase(Ease.OutElastic);
        if (!isStop)
        {
            yield return new WaitForSeconds(0.2f);
        }
        else
        {
            yield return null;
        }
    }


    private void KillAllTweens()
    {
        for (int i = 0; i < numberOfSlots; i++)
        {
            alltweens[i].Kill();
        }
        alltweens.Clear();

    }
    #endregion

}

[Serializable]
public class SlotImage
{
    public List<Image> slotImages = new List<Image>(10);
}
[Serializable]
public class SlotREsultImage
{
    public List<ImageAnimation> slotImages = new List<ImageAnimation>(10);
}
