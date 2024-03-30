using System;
using System.Collections.Generic;

public class Stats 
{
    private int _curHP, _maxHP; 
    public int HP 
    { 
        get { return _curHP; } 
        set { _curHP = value > _maxHP ? _maxHP : value; } 
    }
    public int MaxHP 
    {
        get { return _maxHP; }
        set { _maxHP = value; _curHP = _maxHP < _curHP ? _maxHP : _curHP; }
    }
    public int AD { get; set; }
    public float MS { get; set; }
    public float AS { get; set; }
    public float CR { get; set; }
    public Status status { get; set; }
}