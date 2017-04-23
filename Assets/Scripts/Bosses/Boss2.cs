﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boss2 : BossBase {

    public override int BossOrderID { get { return 2; } }

    public Pillar[] Pillars = new Pillar[5];
    public int maxHP;
    public float curHP;
    public float moveSpeed;

    public int curPhase = 1;
    public float phaseDuration;
    private float phaseStart;

    public float timeBetweenAttacks;
    public float lastAttack;

    public GameObject[] waypoints = new GameObject[6];
    int nextWP = 0;
    bool isLanded = false;
    private int landedPillar = -1; //-1 if in air, pillar number if landed

    public MovingDoor deathDoor;
    private Rigidbody2D rb;
    private new Collider2D collider;
    private SpriteRenderer sprite;
    private Vector2 curV;


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

    void FixedUpdate () {
        if (Time.time > lastAttack + timeBetweenAttacks && !isLanded) {
            //shoot pew pews
            //select 2 pillars.
            List<int> ex = new List<int>(curPhase - 1);
            for (int i = 0; i < 5; i++) {
                if (!Pillars[i].active) {
                    ex.Add(i);
                }
            }
            int pillarChoice = SelectPillar(0, 4, ex);
            ex.Add(pillarChoice);
            //2nd unique pillar
            int pillarChoice2 = SelectPillar(0, 4, ex);

        } else if (Time.time > phaseStart + phaseDuration && !isLanded) {
            //chose pillar to land on
            List<int> ex = new List<int>(curPhase - 1);
            for (int i = 0; i < 5; i++) {
                if (!Pillars[i].active) {
                    ex.Add(i);
                }
            }
            int pillarChoice = SelectPillar(0, 4, ex);
            isLanded = true;
        } else {
            //just fly around n shit, idk

            //if reached a waypoint, set target to be the next one. 
            if (Vector2.Distance(transform.position, waypoints[nextWP].transform.position) < 0.5) {
                nextWP++;
                if (nextWP >= waypoints.Length) {
                    nextWP = 0;
                }
            } else {
                //move towards next WP
                Vector2.SmoothDamp(transform.position, waypoints[nextWP].transform.position, ref curV, moveSpeed, 1000, Time.deltaTime);
            }
        }
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
    public int SelectPillar(int min, int max, List<int> exclude) {
        int targetNum = (int)UnityEngine.Random.Range(min, max - exclude.Count);
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