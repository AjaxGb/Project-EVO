using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boss2 : BossBase {

    public override int BossOrderID { get { return 2; } }

    public Pillar[] Pillars = new Pillar[5];
    public float moveSpeed;
    public float landingSpeed;

    //phases
    public int curPhase = 1;
    public float phaseDuration;
    private float phaseStart;
    
    //attacking
    public float timeBetweenAttacks;
    float lastAttack;
    public int[] targets = new int[2];
    public float chargeTime;
    public float damage = 45;
    public GameObject beam;
    public GameObject beamHit;
    private List<GameObject> beamObjects = new List<GameObject>(4);
    float chargeStart = -1;

    //movement
    public GameObject[] waypoints = new GameObject[6];
    public int nextWP = 0;
    Vector2 targetLoc;
    public enum State {FLYING, LANDED, LANDING, LANDINGPREP };
    public State actionState = State.FLYING;
    private int landedPillar = -1; //-1 if in air, pillar number if landed

	private bool fightStarted = false;
	public MovingDoor noEscapeDoor;
	public MovingDoor noEscapeLedge;
	private Rigidbody2D rb;
    private new Collider2D collider;
    public SpriteRenderer sprite;
    public Animator anim;
    private Vector2 curV;

    // Use this for initialization
    public override void StartAlive() {
        targetLoc = waypoints[0].transform.position;
        rb = GetComponent<Rigidbody2D>();
        phaseStart = Time.time;
        collider = GetComponent<Collider2D>();
    }

    public override void StartDead() {
        Destroy(gameObject);
        //deathDoor.IsOpened = true;
        //deathDoor.SkipAnimation();
    }

    void FixedUpdate () {
		if (!fightStarted) {
			if (SceneLoader.IsInCurrentScene(gameObject)) {
				fightStarted = true;
				noEscapeDoor.IsOpened = false;
				noEscapeLedge.IsOpened = false;
			} else {
				// Do not move until player enters room.
				return;
			}
		}

        //if targets have already been selected and the attack is charged, attack em
        if (chargeStart != -1 && Time.time > chargeStart + chargeTime) {

            //pew pew
            foreach (int t in targets) {
                Pillars[t].Blast(damage);
            }
            //stop charging lasers
            chargeStart = -1;
            lastAttack = Time.time;
            foreach (GameObject g in beamObjects) {
                Destroy(g);
            }
            beamObjects.Clear();
            anim.SetTrigger("Fire");

        } else if (actionState == State.FLYING && chargeStart == -1) {

            //===if its time to shoot===
            if (Time.time > lastAttack + timeBetweenAttacks) {
                //shoot pew pews
                //select 2 pillars.
                List<int> ex = new List<int>(curPhase - 1);
                for (int i = 0; i < 5; i++) {
                    if (!Pillars[i].unBroken) {
                        ex.Add(i);
                    }
                }
                targets[0] = SelectPillar(0, 4, ex);
                ex.Add(targets[0]);
                //2nd unique pillar
                targets[1] = SelectPillar(0, 4, ex);
                chargeStart = Time.time;
                rb.velocity = Vector2.zero;
                //face the midpoint of the two beams
                float midpoint = 0;
                foreach (int i in targets) {
                    midpoint += Pillars[i].GetComponent<Pillar>().landingZone.transform.position.x;
                }
                midpoint /= targets.Length;
                sprite.flipX = transform.position.x > midpoint;
                //INSTANTIATE BEAMS
                Vector3 firstTargetPos = (Vector3)(Pillars[targets[0]].GetComponent<Pillar>().landingZone.transform.position);
                Vector3 secondTargetPos = (Vector3)(Pillars[targets[1]].GetComponent<Pillar>().landingZone.transform.position);
                //beamhits first
                beamObjects.Add(  Instantiate(beamHit, firstTargetPos, Quaternion.identity)  );
                beamObjects.Add(  Instantiate(beamHit, secondTargetPos, Quaternion.identity)  );
                firstTargetPos += new Vector3(0, 0.2f,0);
                secondTargetPos += new Vector3(0, 0.2f, 0);
                //then real beams
                Vector3 laserSource = transform.position + new Vector3(sprite.flipX ? -0.5f : 0.5f, 1.55f, 0);
                GameObject temp = Instantiate(beam, firstTargetPos, Quaternion.LookRotation(Vector3.forward, laserSource - firstTargetPos)   );
                temp.transform.localScale = new Vector3(1, Vector3.Distance(laserSource, firstTargetPos)/6, 1);
                beamObjects.Add(temp);
                temp = Instantiate(beam, secondTargetPos, Quaternion.LookRotation(Vector3.forward, laserSource - secondTargetPos));
                temp.transform.localScale = new Vector3(1, Vector3.Distance(laserSource, secondTargetPos) / 6, 1);
                beamObjects.Add(temp);
                anim.SetTrigger("StartCharge");
            } else 
            //===if its time to land===
            if (Time.time > phaseStart + phaseDuration) {
                //chose pillar to land on
                List<int> ex = new List<int>(curPhase - 1);
                for (int i = 0; i < 5; i++) {
                    if (!Pillars[i].unBroken) {
                        ex.Add(i);
                    }
                }
                landedPillar = SelectPillar(0, 4, ex);
                actionState = State.LANDINGPREP;
                targetLoc = new Vector2(Pillars[landedPillar].landingZone.transform.position.x, Pillars[landedPillar].landingZone.transform.position.y + UnityEngine.Random.Range(4, 7));


            //===flyin around===
            } else 

            //if reached a waypoint, set target to be the next one. 
            if (Vector2.Distance(transform.position, waypoints[nextWP].transform.position) < 0.5) {
                nextWP++;
                if (nextWP >= waypoints.Length) {
                    nextWP = 0;
                }
                //move towards next WP
                targetLoc = waypoints[nextWP].transform.position;
            }
                
            
        } else if (actionState == State.LANDINGPREP) {
            //If it gets close to the spot above the pillar, then set the new destination to be landed on it
            if (Vector2.Distance(targetLoc, transform.position) < 0.25) {
                targetLoc = Pillars[landedPillar].landingZone.transform.position;
                actionState = State.LANDING;
                anim.SetBool("isLanding", true);
            }
        } else if (actionState == State.LANDING) {
            //if it gets close to the landing zone, set it to be landed
            if (Vector2.Distance(targetLoc, transform.position) < 0.1) {
                transform.position = targetLoc;
                rb.velocity = Vector2.zero;
                actionState = State.LANDED;
            }
        }

        //===START MOVEMENT TO TARGET LOC===
        if (actionState != State.LANDED && chargeStart == -1) {
            sprite.flipX = targetLoc.x < transform.position.x;
            if (actionState != State.LANDING)
               rb.velocity = (targetLoc - (Vector2)transform.position).normalized * moveSpeed;
            else
               rb.velocity = (targetLoc - (Vector2)transform.position).normalized * landingSpeed;
        }
    }

    public override void OnDamaged() {
        //phase change checks
        if (actionState == State.LANDED) {
            if (  (CurrHealth < (0.66 * maxHealth) && curPhase <= 1)  ||  (CurrHealth < (0.33 * maxHealth) && curPhase <= 2)  ) {
                curPhase++;
                Pillars[landedPillar].Break();
                phaseStart = Time.time;
                actionState = State.FLYING;
                targetLoc = waypoints[nextWP].transform.position;
                landedPillar = -1;
                anim.SetBool("isLanding", false);
            } 
        }
    }
    
    public override void OnKilled() {
        //repair pillars on death
        foreach (Pillar p in Pillars) {
            if (!p.unBroken) {
                p.UnBreak();
            }
        }
        anim.SetTrigger("death");
        Destroy(gameObject, 4f);
		noEscapeDoor.IsOpened = true;
		noEscapeLedge.IsOpened = true;
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
