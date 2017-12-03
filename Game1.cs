using Barely.SceneManagement;
using Barely.Util;
using LD40.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System.Xml;

namespace LD40
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        
        Scene currentScene;

        Texture2D atlas;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            Config.Resolution = new Point(1280, 720);

            IsMouseVisible = true;
            graphics.PreferredBackBufferWidth = Config.Resolution.X;
            graphics.PreferredBackBufferHeight = Config.Resolution.Y;
            graphics.SynchronizeWithVerticalRetrace = false;
            IsFixedTimeStep = false;
            graphics.ApplyChanges();
            SpriteBatchEx.GraphicsDevice = GraphicsDevice;

            atlas = Content.Load<Texture2D>("graphics");

            XmlDocument tilesetDef = new XmlDocument();
            tilesetDef.Load("Content/tilesets.xml");
            Tileset.LoadTilesets(tilesetDef, atlas);
            
            MapScene mapScene = new MapScene(Content, GraphicsDevice, this);
            currentScene = mapScene;
            SpriteBatchEx.GraphicsDevice = GraphicsDevice;
        }


        protected override void Initialize()
        {
            base.Initialize();

            Window.Title = $"Happy Bot Mess - A game for Ludum Dare 40";

            XmlDocument langXml = new XmlDocument();
            langXml.Load("Content/Language/en.xml");
            Texts.SetTextFile(langXml);

            XmlDocument effectXml = new XmlDocument();
            effectXml.Load("Content/effects.xml");
            Effects.Initialize(effectXml, atlas);

            XmlDocument soundsXml = new XmlDocument();
            soundsXml.Load("Content/Sounds/sounds.xml");
            Sounds.Initialize(Content, soundsXml);

            Window.Position = (new Point(1920, 1080) - Config.Resolution) / new Point(2, 2);
            currentScene.Initialize();
        }


        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            currentScene.LoadContent();
        }


        protected override void UnloadContent()
        {
            currentScene.UnloadContent();
        }


        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            double deltaTime = gameTime.ElapsedGameTime.TotalSeconds;
            Barely.Util.Input.Update();
            Animator.Update(deltaTime);
            Effects.Update(deltaTime);
            currentScene.Update(deltaTime);
            base.Update(gameTime);
        }




        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(new Color(34, 32, 52));

            currentScene.Draw(spriteBatch);

            base.Draw(gameTime);
        }
    }
}
