using System;
using System.Collections.Generic;

public class Stats 
{
    public Stats()
    {
        this._curHP =(int)Mappings.DefaultStats["curHP"];
        this._maxHP = (int)Mappings.DefaultStats["maxHP"];
        this.AD = (int)Mappings.DefaultStats["AD"];
        this.AS = (float)Mappings.DefaultStats["AS"];
        this.CR = (float)Mappings.DefaultStats["CR"];
        this.MS = (float)Mappings.DefaultStats["MS"];
        this.status = (Status)Mappings.DefaultStats["status"];
    }
    public Stats(Stats other) 
    {
        this._curHP = other._curHP;
        this._maxHP = other._maxHP;
        this.AD = other.AD;
        this.AS = other.AS;
        this.CR = other.CR;
        this.MS = other.MS;
        this.status = other.status;
    }
    private int _curHP, _maxHP; 
    public int HP 
    { 
        get { return _curHP; } 
        set { _curHP = value > _maxHP ? _maxHP : (value < 0 ? 0 : value); } 
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