using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boss2 : BossBase {

    public override int BossOrderID { get { return 2; } }

    public Pillar[] Pillars = new Pillar[5];
    public int maxHP;
    public float curHP;
    public int curPhase = 1;
    public float phaseDuration;
    private float phaseStart;
    private int landedPillar = -1; //-1 if in air, pillar number if landed

    public MovingDoor deathDoor;
    private Rigidbody2D rb;
    private new Collider2D collider;
    private SpriteRenderer sprite;


    // Use this for initialization
    public override void StartAlive() {
        rb = GetComponent<Rigidbody2D>();
        phaseStart = Time.time;
        collider = GetComponent<Collider2D>();
        sprite = GetComponent<SpriteRenderer>();
    }

    public override void StartDead() {
        Destroy(gameObject);
        deathDoor.IsOpened = true;
        deathDoor.SkipAnimation();
    }

    // Update is called once per frame
    void Update () {
        
	}

    public void OnDamage() {
        //phase change checks
        if (curHP < (0.66 * maxHP) && curPhase == 1) {
            curPhase = 2;
            Pillars[landedPillar].Break();
            phaseStart = Time.time;
        } else if (curHP < (0.33 * maxHP) && curPhase == 2) {
            curPhase = 3;
            Pillars[landedPillar].Break();
            phaseStart = Time.time;
        }
    }
    
    public void OnKill() {
        //repair pillars on death
        foreach (Pillar p in Pillars) {
            if (!p.active) {
                p.UnBreak();
            }
        }
        Destroy(transform.gameObject, 4f);
        deathDoor.IsOpened = true;
        collider.enabled = false;
    }

    //selects a pillar. Exlude must not be larger than the available numbers. Exclude must be sorted
    public int SelectPillar(int min, int max, int[] exclude) {
        int targetNum = (int)UnityEngine.Random.Range(min, max - exclude.Length);
        foreach (int ex in exclude) {
            if (targetNum >= ex) {
                targetNum++;
            } else {
                break;
            }
        }
        return targetNum;
    }

}
