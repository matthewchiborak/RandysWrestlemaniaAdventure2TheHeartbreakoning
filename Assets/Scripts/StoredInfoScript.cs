﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using System.IO;
using System.Text;

public class StoredInfoScript : MonoBehaviour {

    public static StoredInfoScript persistantInfo;

    public string currentScene;
    public Vector3 lastLoadLocation = new Vector3(0, 0, 0);
    public Vector3 lastLoadRotation = new Vector3(0, 0, 0);

    public float currentHealth = 100;
    public float maxHealth = 100;
    public int itemSelected = 0;
    public float speedMulitplier = 1.0f;

    public GameObject shawnMichaels;
    public Transform shawnMichaelsTransform;
    public ShawnMichaelsControl shawnMichaelsScript;
    public Animator shawnMichaelsAnim;

    private bool[] abilityEnabled = new bool[6];

    public Material[] itemMaterials = new Material[7];
    public Image itemImage;
    public Text itemAmount;
    private int bandageAmount = 2;
    private int pillsAmount = 2;
    private int beerAmount = 3;
    private int maxItems = 6;

    //For block out screen when load new area
    public Image loadScreen;
    public Text loadText;

    //Progress level for triggering cutscene, boss fights, and locked doors
    //private int progressLevel = 0;
    public int progressLevel = 0;

    //Last know position of player
    public Vector3 lastPosition = new Vector3(10000f, 10000f, 10000f);
    public Vector3 resetPosition = new Vector3(10000f, 10000f, 10000f);

    //Music Fading
    private float fadeSpeed = 7f;
    private float musicFadeSpeed = 1f;
    public AudioSource backgroundMusic;
    public AudioSource panicMusic;

    //Healthbar
    public Image healthBar;

    //Music Stuff
    public AudioClip[] BGTracks;
    public int currentTrack;

    public bool ignorePlayer = false;

    //Boss stuff
    private float maxBossHealth = 100f;
    private float currentBossHealth = 100f;
    public Image bossBarBack;
    public Image bossBarFront;
    public Text bossName;

    public AudioSource cookieCrunch;

    private bool paused = false;
    public GameObject pausedScreen;
    public Image[] roomImages;//14

    public CutscenePlayer theScenePlayer;

    public bool death = false;
    private float deathTimer = 0;
    private float TimeForDie = 6f;

    public Image GameOverScreen;
    public AudioSource GameOverSong;

    private Vector2 startVect;
    private Vector2 endVect;

    void Start()
    {
        Cursor.visible = false;

        string assetText;

        using (var streamReader = new StreamReader("Assets/Resources/Save.txt", Encoding.UTF8))
        {
            assetText = streamReader.ReadToEnd();
        }

        progressLevel = Int32.Parse(assetText);

        //Enable item accordingly
        if(progressLevel > 6)
        {
            enableItem(2);
        }
        if (progressLevel > 14)
        {
            enableItem(4);
        }
        if (progressLevel > 25)
        {
            enableItem(3);
        }
        if (progressLevel > 32)
        {
            enableItem(5);
        }

        startVect = new Vector2(1, 1);
        endVect = new Vector2(0, 0);
    }

    public void StartBoss(string name)
    {
        bossBarBack.enabled = true;
        bossBarFront.enabled = true;
        bossName.enabled = true;
        bossName.text = name;
        currentBossHealth = maxBossHealth;
        bossBarFront.transform.localScale = new Vector3(currentBossHealth / maxBossHealth, 1, 1);
    }

    public void EndBoss()
    {
        if (progressLevel == 5)
        {
            enableItem(2);
        }
        if (progressLevel == 15)
        {
            enableItem(4);
        }

        currentBossHealth = maxBossHealth;
        bossBarBack.enabled = false;
        bossBarFront.enabled = false;
        bossName.enabled = false;
        switchTracks();
        //progressLevel++;
        IncreaseProgress();
    }

    public void PlayCutscene(int sceneToPlay)
    {
        //progressLevel++;
        IncreaseProgress();

        //Turn off all audiosources
        backgroundMusic.Stop();
        panicMusic.Stop();
        shawnMichaelsScript.footSteps.volume = 0;
        shawnMichaelsScript.itemSource.volume = 0;

        theScenePlayer.PlayCutscene(sceneToPlay);
    }

    public void CutsceneJustEnded()
    {
        //Turn on all audiosources
        backgroundMusic.Play();
        panicMusic.Play();
        shawnMichaelsScript.footSteps.volume = 1;
        shawnMichaelsScript.itemSource.volume = 1;

        //Boss happens afterwards
        if (progressLevel == 3)
        {
            //progressLevel++;
            IncreaseProgress();
        }
        if (progressLevel == 12)
        {
            //progressLevel++;
            IncreaseProgress();
        }
        //if (progressLevel == 15)
        //{
        //    progressLevel++;
        //}
        if (progressLevel == 15)
        {
            enableItem(4);
        }
        if (progressLevel == 17)
        {
            playBearMusic();
            //progressLevel++;
            IncreaseProgress();
        }
        if (progressLevel == 22)
        {
            enableItem(2);
            //progressLevel++;
            IncreaseProgress();
        }
        if (progressLevel == 26)
        {
            enableItem(3);
            //progressLevel++;
            //IncreaseProgress();
        }
        if (progressLevel == 29)
        {
            //progressLevel++;
            IncreaseProgress();
        }
        if (progressLevel == 33)
        {
            enableItem(5);
        }
    }

    public void hitBoss(float damage)
    {
        currentBossHealth -= damage;
        if (currentBossHealth < 0)
        {
            currentBossHealth = 0;
        }
        bossBarFront.transform.localScale = new Vector3(currentBossHealth / maxBossHealth, 1, 1);
    }

    public Animator getPlayerAnim()
    {
        return shawnMichaelsScript.GetPlayerAnim();
    }
    public Transform getPlayerTransform()
    {
        return shawnMichaelsTransform;
    }
    public GameObject getPlayerGameObject()
    {
        return shawnMichaels;
    }
    public ShawnMichaelsControl getPlayerScript()
    {
        return shawnMichaelsScript;
    }

    public string getCurrentScene()
    {
        return currentScene;
    }

    public float getBossPercentage()
    {
        return currentBossHealth / maxBossHealth;
    }



    public void hitByBullet()
    {
        currentHealth -= 10;

        if (currentHealth <= 0)
        {
            currentHealth = 0;
        }

        healthBar.transform.localScale = new Vector3(currentHealth / maxHealth, 1, 1);

        //Game over if game is over
        if (currentHealth == 0)
        {
            gameOver();
        }
    }

    public void hitByCactus()
    {
        currentHealth -= 10;

        if (currentHealth <= 0)
        {
            currentHealth = 0;
        }

        healthBar.transform.localScale = new Vector3(currentHealth / maxHealth, 1, 1);

        //Game over if game is over
        if (currentHealth == 0)
        {
            gameOver();
        }
    }

    public void hitByPillow()
    {
        speedMulitplier = 0.1f;
        cookieCrunch.time = 1.3f;
        cookieCrunch.Play();
    }

    public void hitByDust()
    {
        currentHealth -= 7;

        if (currentHealth <= 0)
        {
            currentHealth = 0;
        }

        healthBar.transform.localScale = new Vector3(currentHealth / maxHealth, 1, 1);

        //Game over if game is over
        if (currentHealth == 0)
        {
            gameOver();
        }
    }

    public void hitBySkeleton()
    {
        currentHealth -= 0.1f;

        if (currentHealth <= 0)
        {
            currentHealth = 0;
        }

        healthBar.transform.localScale = new Vector3(currentHealth / maxHealth, 1, 1);

        //Game over if game is over
        if (currentHealth == 0)
        {
            gameOver();
        }
    }

    public void MapToggle()
    {
        if (!paused)
        {
            paused = true;
            //Stop time
            Time.timeScale = 0;
            pausedScreen.SetActive(true);

            if (currentScene == "entrance")
            {
                roomImages[0].color = Color.yellow;
            }
            else
            {
                roomImages[0].color = Color.white;
            }
            if (currentScene == "test")
            {
                roomImages[1].color = Color.yellow;
            }
            else
            {
                roomImages[1].color = Color.white;
            }
            if (currentScene == "food")
            {
                roomImages[2].color = Color.yellow;
            }
            else
            {
                roomImages[2].color = Color.white;
            }
            if (currentScene == "hallway")
            {
                roomImages[3].color = Color.yellow;
                //Also the other one
                roomImages[13].color = Color.yellow;
            }
            else
            {
                roomImages[3].color = Color.white;
                roomImages[13].color = Color.white;
            }
            if (currentScene == "backstage")
            {
                roomImages[4].color = Color.yellow;
            }
            else
            {
                roomImages[4].color = Color.white;
            }
            if (currentScene == "bar")
            {
                roomImages[5].color = Color.yellow;
            }
            else
            {
                roomImages[5].color = Color.white;
            }
            if (currentScene == "gym")
            {
                roomImages[6].color = Color.yellow;
            }
            else
            {
                roomImages[6].color = Color.white;
            }
            if (currentScene == "shop")
            {
                roomImages[7].color = Color.yellow;
            }
            else
            {
                roomImages[7].color = Color.white;
            }
            if (currentScene == "washroom")
            {
                roomImages[8].color = Color.yellow;
            }
            else
            {
                roomImages[8].color = Color.white;
            }
            if (currentScene == "garage" || currentScene == "testground")
            {
                roomImages[9].color = Color.yellow;
            }
            else
            {
                roomImages[9].color = Color.white;
            }
            if (currentScene == "cubicles")
            {
                roomImages[10].color = Color.yellow;
            }
            else
            {
                roomImages[10].color = Color.white;
            }
            if (currentScene == "office")
            {
                roomImages[11].color = Color.yellow;
            }
            else
            {
                roomImages[11].color = Color.white;
            }
            if (currentScene == "stairway")
            {
                roomImages[12].color = Color.yellow;
            }
            else
            {
                roomImages[12].color = Color.white;
            }
        }
        else
        {
            paused = false;
            Time.timeScale = 1.0f;
            pausedScreen.SetActive(false);
        }
    }

    public void gameOver()
    {
        if (!death)
        {
            GameOverScreen.enabled = true;
            Time.timeScale = 0.2f;
            death = true;
            GameOverSong.Play();
        }
    }

    public void ReloadFromGameOver()
    {
        Time.timeScale = 1.0f;

        StoredInfoScript.persistantInfo.blockScreen();

        currentHealth = maxHealth;
        healthBar.transform.localScale = new Vector3(currentHealth / maxHealth, 1, 1);

        currentBossHealth = maxBossHealth;
        bossBarFront.transform.localScale = new Vector3(currentBossHealth / maxBossHealth, 1, 1);

        if(progressLevel == 5)
        {
            progressLevel = 2;
        }

        if (progressLevel == 24)
        {
            progressLevel = 21;
        }

        //other.gameObject.GetComponentInParent<Transform>().position = playerLocation;
        shawnMichaels.transform.position = lastLoadLocation;
        //other.gameObject.GetComponentInParent<Transform>().rotation = Quaternion.Euler(playerRotation);
        shawnMichaels.transform.rotation = Quaternion.Euler(lastLoadRotation);
        //SceneManager.LoadScene("fight", LoadSceneMode.Single);

        StoredInfoScript.persistantInfo.lastPosition = StoredInfoScript.persistantInfo.resetPosition;

        SceneManager.LoadScene(currentScene, LoadSceneMode.Single);
    }

    public void useHealthPack()
    {
        currentHealth += 50;

        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }

        healthBar.transform.localScale = new Vector3(currentHealth / maxHealth, 1, 1);
    }

    public void resetMusic()
    {
        backgroundMusic.volume = 1f;
        panicMusic.volume = 0f;
    }

    public void PlayBossMusic()
    {
        currentTrack = 0;
        backgroundMusic.clip = BGTracks[0];
        backgroundMusic.Play();
    }

    public void playFinalBossMusic()
    {
        currentTrack = 2;
        backgroundMusic.clip = BGTracks[2];
        backgroundMusic.Play();
    }

    public void playBearMusic()
    {
        currentTrack = 3;
        backgroundMusic.clip = BGTracks[3];
        backgroundMusic.Play();
    }

    public void switchTracks()
    {
        if (currentScene == "backstage" && currentTrack != 16)
        {
            currentTrack = 16;
            backgroundMusic.clip = BGTracks[16];
            backgroundMusic.Play();
        }
        else if (currentScene == "bar")
        {
            backgroundMusic.clip = BGTracks[1];
            currentTrack = 1;
            backgroundMusic.Play();
        }
        else if (currentScene == "cubicles" && currentTrack != 16)
        {
            currentTrack = 16;
            backgroundMusic.clip = BGTracks[16];
            backgroundMusic.Play();
        }
        else if (currentScene == "entrance" && currentTrack != 16)
        {
            currentTrack = 16;
            backgroundMusic.clip = BGTracks[16];
            backgroundMusic.Play();
        }
        else if (currentScene == "food" && currentTrack != 16)
        {
            currentTrack = 16;
            backgroundMusic.clip = BGTracks[16];
            backgroundMusic.Play();
        }
        else if (currentScene == "forest")
        {
            backgroundMusic.clip = BGTracks[5];
            currentTrack = 5;
            backgroundMusic.Play();
        }
        else if (currentScene == "garage" && currentTrack != 16)
        {
            currentTrack = 16;
            backgroundMusic.clip = BGTracks[16];
            backgroundMusic.Play();
        }
        else if (currentScene == "gym" && currentTrack != 16)
        {
            currentTrack = 16;
            backgroundMusic.clip = BGTracks[16];
            backgroundMusic.Play();
        }
        else if (currentScene == "hallway" && currentTrack != 16)
        {
            currentTrack = 16;
            backgroundMusic.clip = BGTracks[16];
            backgroundMusic.Play();
        }
        else if (currentScene == "lockerroom" && currentTrack != 16)
        {
            currentTrack = 16;
            backgroundMusic.clip = BGTracks[16];
            backgroundMusic.Play();
        }
        else if (currentScene == "office")
        {
            backgroundMusic.clip = BGTracks[10];
            currentTrack = 10;
            backgroundMusic.Play();
        }
        else if (currentScene == "outside")
        {
            backgroundMusic.clip = BGTracks[11];
        }
        else if (currentScene == "outsideDay")
        {
            backgroundMusic.clip = BGTracks[12];
        }
        else if (currentScene == "shop" && currentTrack != 16)
        {
            currentTrack = 16;
            backgroundMusic.clip = BGTracks[16];
            backgroundMusic.Play();
        }
        else if (currentScene == "space")
        {
            backgroundMusic.clip = BGTracks[14];
        }
        else if (currentScene == "stairway")
        {
            backgroundMusic.clip = BGTracks[15];
            currentTrack = 15;
            backgroundMusic.Play();
        }
        else if (currentScene == "test" && currentTrack != 16)
        {
            currentTrack = 16;
            backgroundMusic.clip = BGTracks[16];
            backgroundMusic.Play();
        }
        else if (currentScene == "void")
        {
            backgroundMusic.clip = BGTracks[17];
            currentTrack = 17;
            backgroundMusic.Play();
        }
        else if (currentScene == "washroom")
        {
            backgroundMusic.clip = BGTracks[10];
            currentTrack = 10;
            backgroundMusic.Play();
        }
        else if (currentScene == "testground")
        {
            backgroundMusic.clip = BGTracks[0];
            currentTrack = 0;
            backgroundMusic.Play();
        }
    }

    public void MusicFading()
    {
        if (lastPosition != resetPosition)
        {
            backgroundMusic.volume = Mathf.Lerp(backgroundMusic.volume, 0f, musicFadeSpeed * Time.deltaTime);
            panicMusic.volume = Mathf.Lerp(panicMusic.volume, 1f, musicFadeSpeed * Time.deltaTime);
        }
        else
        {
            backgroundMusic.volume = Mathf.Lerp(backgroundMusic.volume, 1f, musicFadeSpeed * Time.deltaTime);
            panicMusic.volume = Mathf.Lerp(panicMusic.volume, 0f, musicFadeSpeed * Time.deltaTime);
        }
    }

    public int getProgressLevel()
    {
        return progressLevel;
    }

    public void blockScreen()
    {
        loadScreen.enabled = true;
        loadText.enabled = true;
    }

    public void showScreen()
    {
        loadScreen.enabled = false;
        loadText.enabled = false;
    }

    public GameObject getGameObject()
    {
        return gameObject;
    }

    void Awake()
    {
        if (persistantInfo == null)
        {
            DontDestroyOnLoad(gameObject);
            persistantInfo = this;
            abilityEnabled[0] = true; //Only 1 and 0 will be true when done testing
            abilityEnabled[1] = true;
            abilityEnabled[2] = false;
            abilityEnabled[3] = false;
            abilityEnabled[4] = false;
            abilityEnabled[5] = false;
            UnityEngine.Random.InitState(System.DateTime.Now.Second);
            itemImage.material = itemMaterials[0];
            itemAmount.text = bandageAmount.ToString();
        }
        else if (persistantInfo != this)
        {
            Destroy(persistantInfo);
        }
    }

    public void IncreaseProgress()
    {

        progressLevel++;

        if ((progressLevel < 14) || (progressLevel > 19 && progressLevel < 26) || (progressLevel > 27))
        {
            //I should do checks so I don't screw myself out of progress
            System.IO.File.WriteAllText("Assets/Resources/Save.txt", progressLevel.ToString());
        }

        
    }

    public void enableItem(int itemNumber)
    {
        abilityEnabled[itemNumber] = true;
    }

    public void updateUI()
    {

    }

    public void pickupItem(int itemId)
    {
        if (itemId == 0)
        {
            if(bandageAmount >= maxItems)
            {
                return;
            }

            bandageAmount++;
            if (itemSelected == 0)
            {
                itemAmount.text = bandageAmount.ToString();
            }
        }
        if (itemId == 1)
        {
            if (pillsAmount >= maxItems)
            {
                return;
            }

            pillsAmount++;
            if (itemSelected == 1)
            {
                itemAmount.text = pillsAmount.ToString();
            }
        }
        if (itemId == 4)
        {
            if (beerAmount >= maxItems)
            {
                return;
            }

            beerAmount++;
            if (itemSelected == 4)
            {
                itemAmount.text = beerAmount.ToString();
            }
        }
    }

    public bool checkIfEnough()
    {
        int itemNo = itemSelected;

        if (itemNo == 0)
        {
            if(bandageAmount > 0)
            {
                bandageAmount--;
                itemAmount.text = bandageAmount.ToString();

                //Increase the health
                currentHealth += 50;
                if(currentHealth > maxHealth)
                {
                    currentHealth = maxHealth;
                }

                return true;
            }
        }
        else if (itemNo == 1)
        {
            if (pillsAmount > 0)
            {
                pillsAmount--;
                itemAmount.text = pillsAmount.ToString();
                speedMulitplier = 2.0f;
                return true;
            }
        }
        else if (itemNo == 4)
        {
            if (beerAmount > 0)
            {
                beerAmount--;
                itemAmount.text = beerAmount.ToString();
                return true;
            }
        }
        else
        {
            return true;
        }

        return false;
    }

    public void selectItem(int item)
    {
        //Check if the item is enabled
        if(!abilityEnabled[item])
        //if (abilityEnabled[item])
        {
            return;
        }

        itemSelected = item;

        //Swap the UI
        itemImage.material = itemMaterials[item];

        //Swap the quantity if its a resource that can be used up
        if(item == 0)
        {
            itemAmount.text = bandageAmount.ToString();
        }
        else if(item == 1)
        {
            itemAmount.text = pillsAmount.ToString();
        }
        else if (item == 4)
        {
            itemAmount.text = beerAmount.ToString();
        }
        else
        {
            itemAmount.text = "";
        }
    }

    // Use this for initialization
    //   void Start () {

    //}

    

	//// Update is called once per frame
	void Update ()
    {
        MusicFading();

        //Create a way for the player of quitting the game
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (progressLevel == 5 || progressLevel == 4 || progressLevel == 3 || progressLevel == 2)
            {
                System.IO.File.WriteAllText("Assets/Resources/Save.txt", "1");
            }

            if (progressLevel == 24 || progressLevel == 23 || progressLevel == 22 || progressLevel == 21)
            {
                System.IO.File.WriteAllText("Assets/Resources/Save.txt", "20");
            }

            if (progressLevel == 29 || progressLevel == 30 || progressLevel == 31 || progressLevel == 32)
            {
                System.IO.File.WriteAllText("Assets/Resources/Save.txt", "28");
            }

            Application.Quit();
        }

        if (death)
        {
            Time.timeScale = 0.2f;

            if(deathTimer < TimeForDie)
            {
                deathTimer += Time.deltaTime;
                GameOverScreen.color = new Color(1, 1, 1, deathTimer * 2 / TimeForDie);
                //backgroundMusic.volume = Vector2.Lerp(startVect, endVect, deathTimer * 2 / TimeForDie).x;
                //panicMusic.volume = Vector2.Lerp(startVect, endVect, deathTimer * 2 / TimeForDie).x;
                backgroundMusic.volume = 0;
                panicMusic.volume = 0;
            }
            else
            {
                GameOverScreen.color = new Color(1, 1, 1, 0);
                GameOverScreen.enabled = false;
                death = false;
                deathTimer = 0;
                ReloadFromGameOver();
                backgroundMusic.volume = 1;
                panicMusic.volume = 0;
            }
        }
	}
}
