using Barely.SceneManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using LD40.World;
using LD40.Actors;
using Barely.Util;
using BarelyUI;
using System.Xml;
using LD40.Interface;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Audio;

namespace LD40
{
    public class MapScene : Scene
    {

        enum SceneState
        {
            MainMenu,
            MapFocus,
            MapTransition
        }

        Song song;
        Texture2D atlas;
        Map currentMap;
        SceneState state = SceneState.MainMenu;
        DamagePopup damagePopup;

        Canvas mapCanvas;
        Canvas transitionCanvas;
        Canvas mainMenuCanvas;

        List<String[]> mapData;
        int levelIndex = 0;

        bool gameFailed = false;

        public MapScene(ContentManager Content, GraphicsDevice GraphicsDevice, Game game)
                 : base(Content, GraphicsDevice, game)
        {
            atlas = Content.Load<Texture2D>("graphics");
            camera = new Camera(GraphicsDevice.Viewport, 4f, 1f);
            XmlDocument uiDef = new XmlDocument();
            uiDef.Load("Content/ui.xml");
            mapCanvas           = new Canvas(Content, uiDef, Config.Resolution, GraphicsDevice);
            transitionCanvas    = new Canvas(Content, uiDef, Config.Resolution, GraphicsDevice);
            mainMenuCanvas      = new Canvas(Content, uiDef, Config.Resolution, GraphicsDevice);
        }

        public override void Initialize()
        {
            song = Content.Load<Song>("Music/firstSong");
            MediaPlayer.Play(song);
            MediaPlayer.IsRepeating = true;
            MediaPlayer.Volume = 0.25f;

            LoadMapsFromFile();
            CreateUI();
        }

        void LoadMapsFromFile()
        {
            mapData = new List<string[]>(4);
            string[] lines = System.IO.File.ReadAllLines("Content/level.txt");

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                if (line.Length == 0 || line[0] == '#')
                    continue;

                //Level name found
                if (line[0] == '+')
                {
                    int k = i + 1;
                    while(k < lines.Length && lines[k][0] != '+')
                    {
                        k++;
                    }
                    string[] level = new string[k - i];
                    for(k = 0; k < level.Length; k++)
                    {
                        level[k] = lines[i + k];
                    }
                    mapData.Add(level);
                    i += k - 1;
                }
            }

        }

        void StartNewGame()
        {
            state = SceneState.MapTransition;
            gameFailed = false;
            levelIndex = 0;
            currentMap = new Map(mapData[levelIndex], atlas, Content, this);
            transitionText.ShowText("level0Intro");
            camera.position = new Vector2(95, 85);
        }

        public void MapFinished()
        {
            levelIndex++;
            if (levelIndex < mapData.Count)
            {
                currentMap = new Map(mapData[levelIndex], atlas, Content, this);
            }
            else
                currentMap = null;

            state = SceneState.MapTransition;
            ShowBetweenLevelText();
        }

        public void MapFailed()
        {
            state = SceneState.MapTransition;
            gameFailed = true;
            ShowBetweenLevelText();            
        }

        void TransitionContinueClicked()
        {
            if (!gameFailed && levelIndex < mapData.Count)
            {
                state = SceneState.MapFocus;
                camera.position = new Vector2(95, 85);
            }
            else
            {
                BackToMenu();
            }
        }

        public Camera GetCamera()
        {
            return camera;
        }

        void BackToMenu()
        {
            state = SceneState.MainMenu;
            currentMap = null;
        }

        public void ShowBetweenLevelText()
        {
            if (gameFailed)
            {
                transitionText.ShowText("gameOver", BackToMenu);
            }
            else if(levelIndex >= mapData.Count)
            {
                transitionText.ShowText("gameFinished");
            }
            else
            {
                transitionText.ShowText($"level{levelIndex}Intro");
            }
        }

        public override void LoadContent()
        {

        }

        public override void UnloadContent()
        {

        }

        public override void Update(double deltaTime)
        {            
            CameraInput(deltaTime);
            mapCanvas.Update((float)deltaTime);
            if(state == SceneState.MapFocus)
            {
                currentMap.Update(deltaTime, camera, mapCanvas);
            } else if(state == SceneState.MapTransition)
            {
                transitionCanvas.HandleInput();
                transitionCanvas.Update((float)deltaTime);
            } else if(state == SceneState.MainMenu)
            {
                mainMenuCanvas.HandleInput();
                mainMenuCanvas.Update((float)deltaTime);
            }
        }

        protected override void CameraInput(double deltaTime)
        {
            if (state == SceneState.MapFocus)
            {
                Vector2 move = currentMap.CameraInput(deltaTime, camera);
                camera.Update(deltaTime, move);
            }
            else
            {
                //dont know if in transition there is input, maybe i will tween some animation shit
                camera.Update(deltaTime, Vector2.Zero);
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {

            if (state == SceneState.MainMenu)
            {
                spriteBatch.Begin(SpriteSortMode.Immediate, samplerState: SamplerState.PointWrap, transformMatrix: camera.uiTransform);
                mainMenuCanvas.Render(spriteBatch);
                spriteBatch.End();
            }
            else if(state == SceneState.MapTransition)
            {
                spriteBatch.Begin(SpriteSortMode.Immediate, samplerState: SamplerState.PointWrap, transformMatrix: camera.uiTransform);
                transitionCanvas.Render(spriteBatch);
                spriteBatch.End();
            }
            else if(state == SceneState.MapFocus)
            {
                spriteBatch.Begin(SpriteSortMode.Immediate, samplerState: SamplerState.PointWrap, transformMatrix: camera.Transform);

                currentMap.Draw(spriteBatch);
                Effects.Render(spriteBatch);
                spriteBatch.End();

                spriteBatch.Begin(SpriteSortMode.Immediate, samplerState: SamplerState.PointWrap, transformMatrix: camera.uiTransform);
                mapCanvas.Render(spriteBatch);
                spriteBatch.End();

            }

        }


        MapTransitionText transitionText;
        MainMenu mainMenu;

        private void CreateUI()
        {
            mainMenu = new MainMenu(Content, StartNewGame, game.Exit);
            mainMenuCanvas.AddChild(mainMenu);
            mainMenuCanvas.Finish();

            Dictionary<string, SoundEffect> sounds = new Dictionary<string, SoundEffect>()
            {
                { "level0Intro", Content.Load<SoundEffect>("Sounds/level0Intro") },
                { "level1Intro", Content.Load<SoundEffect>("Sounds/level1Intro") },
                { "level2Intro", Content.Load<SoundEffect>("Sounds/level2Intro") },
                { "gameFinished", Content.Load<SoundEffect>("Sounds/gameFinished") },
                { "gameOver", Content.Load<SoundEffect>("Sounds/gameOver") },
            };
            transitionText = new MapTransitionText(TransitionContinueClicked, BackToMenu, game.Exit, sounds);
            transitionCanvas.AddChild(transitionText);
            transitionCanvas.Finish();


            mapCanvas.AddChild(new UnitPanel(() => { if (currentMap != null) return currentMap.GetSelectedUnit(); else return null; }, mapCanvas.sprites));
            mapCanvas.AddChild(new EnemyInfoPanel(() => { if (currentMap != null) return currentMap.GetMouseOverEnemy(); else return null; }));

            damagePopup = new DamagePopup(camera);
            Map.damagePopup = damagePopup;
            mapCanvas.AddChild(damagePopup);


            HorizontalLayout mainButtons = new HorizontalLayout();
            mainButtons.SetMargin(6).Padding = new Point(6, 6);
            mainButtons.SetLayoutSize(LayoutSize.WrapContent, LayoutSize.FixedSize);
            mainButtons.SetFixedHeight(44);
            mainButtons.SetFixedWidth(270);
            mainButtons.sprite = mapCanvas.sprites["panel"];

            Button menuButton = new Button(mapCanvas.sprites["menu"]);
            menuButton.SetFixedSize(32, 32);

            Button musicButton = new Button(Config.musicPlaying ? mapCanvas.sprites["musicOn"] : mapCanvas.sprites["musicOff"]);
            musicButton.SetFixedSize(32, 32);
            musicButton.OnMouseClick = () => {
                Config.musicPlaying = !Config.musicPlaying;
                if (Config.musicPlaying)
                    musicButton.sprite = mapCanvas.sprites["musicOn"];
                else
                    musicButton.sprite = mapCanvas.sprites["musicOff"];
            };

            Button nextTurnButton = new Button(mapCanvas.sprites["nextTurn"]);
            nextTurnButton.SetFixedSize(32, 32);
            nextTurnButton.OnMouseClick = () => { if (currentMap != null) currentMap.EndTurn(); };

            KeyValueText turnText = new KeyValueText("turns", "12");
            turnText.SetLayoutSizeForBoth(LayoutSize.MatchParent);
            turnText.SetValueTextUpdate(() => { return currentMap != null ? currentMap.turnCounter.ToString() : ""; });
            mainButtons.AddChild(menuButton, musicButton, nextTurnButton, turnText);

            mapCanvas.AddChild(mainButtons);


            EnemyTurnSign enemyTurnSign = new EnemyTurnSign(() => { return currentMap; });

            mapCanvas.AddChild(enemyTurnSign);

            ControlsPanel controls = new ControlsPanel(mapCanvas.sprites);
            mapCanvas.AddChild(controls);

            mapCanvas.Finish();
        }      

    }
      
}
