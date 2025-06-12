using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomeResultGenrator : MonoBehaviour
{


    [SerializeField] public List<Payline> payline;
    private List<List<int>> Reel;
    private int ReelRowLen = 3;
    private int ReelColLen = 5;
    private int TotalIcons = 12;
    [Header("DummyData")]
    public List<Payline> DummyReel;
    public List<int> lines;
    public List<Payline> SymbolstoEmit;
    public double winAmount;
    public List<double> bets;
    public float balance;


}

[Serializable]
public class Payline
{
    public List<int> symbol;
}

