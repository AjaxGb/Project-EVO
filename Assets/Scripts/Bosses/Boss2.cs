using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boss2 : BossBase {

    public override int BossOrderID { get { return 2; } }

    public Pillar[] Pillars = new Pillar[5];
    public int maxHP;
    public float curHP;
    public float moveSpeed;

    //phases
    public int curPhase = 1;
    public float phaseDuration;
    private float phaseStart;
    
    //attacking
    public float timeBetweenAttacks;
    public float lastAttack;
    public int[] targets = new int[2];
    public float chargeTime;
    float chargeStart = -1;

    //movement
    public GameObject[] waypoints = new GameObject[6];
    int nextWP = 0;
    Vector2 targetLoc;
    public enum State {FLYING, LANDED, LANDING, LANDINGPREP };
    public State actionState = State.FLYING;
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
        //if targets ahve already been selected, attack em
        if (chargeStart != -1 && Time.time >  + chargeTime) {

            chargeStart = -1;
        }

        if (actionState == State.FLYING) {

            //===if its time to shoot===
            if (Time.time > lastAttack + timeBetweenAttacks) {
                //shoot pew pews
                //select 2 pillars.
                List<int> ex = new List<int>(curPhase - 1);
                for (int i = 0; i < 5; i++) {
                    if (!Pillars[i].active) {
                        ex.Add(i);
                    }
                }
                targets[0] = SelectPillar(0, 4, ex);
                ex.Add(targets[0]);
                //2nd unique pillar
                targets[1] = SelectPillar(0, 4, ex);
                chargeStart = Time.time;

            //===if its time to land===
            } else if (Time.time > phaseStart + phaseDuration) {
                //chose pillar to land on
                List<int> ex = new List<int>(curPhase - 1);
                for (int i = 0; i < 5; i++) {
                    if (!Pillars[i].active) {
                        ex.Add(i);
                    }
                }
                landedPillar = SelectPillar(0, 4, ex);
                actionState = State.LANDINGPREP;
                targetLoc = new Vector2(Pillars[landedPillar].landingZone.transform.position.x, Pillars[landedPillar].landingZone.transform.position.y + UnityEngine.Random.Range(4, 7));


            //===flyin around===
            } else {
                //if reached a waypoint, set target to be the next one. 
                if (Vector2.Distance(transform.position, waypoints[nextWP].transform.position) < 0.5) {
                    nextWP++;
                    if (nextWP >= waypoints.Length) {
                        nextWP = 0;
                    }
                }
                //move towards next WP
                targetLoc = waypoints[nextWP].transform.position;
            }
        } else if (actionState == State.LANDINGPREP) {
            //If it gets close to the spot above the pillar, then set the new destination to be landed on it
            if (Vector2.Distance(targetLoc, transform.position) < 0.25) {
                targetLoc = Pillars[landedPillar].landingZone.transform.position;
                actionState = State.LANDING;
            }
        } else if (actionState == State.LANDING) {
            //if it gets close to the landing zone, set it to be landed
            if (Vector2.Distance(targetLoc, transform.position) < 0.1) {
                transform.position = targetLoc;
                actionState = State.LANDED;
            }
        }

        //===START MOVEMENT TO TARGET LOC===
        if(actionState != State.LANDED)
            Vector2.SmoothDamp(transform.position, targetLoc, ref curV, moveSpeed, 1000, Time.deltaTime);
    }

    public void OnDamage() {
        //phase change checks
        if (actionState == State.LANDED) {
            if (  (curHP < (0.66 * maxHP) && curPhase <= 1)  ||  (curHP < (0.33 * maxHP) && curPhase <= 2)  ) {
                curPhase++;
                Pillars[landedPillar].Break();
                phaseStart = Time.time;
                actionState = State.FLYING;
                landedPillar = -1;
            } 
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
