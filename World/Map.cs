using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using LD40.World;
using LD40.Actors;
using Barely.Util;
using System.Xml;
using Microsoft.Xna.Framework.Input;
using System.Diagnostics;
using Glide;
using BarelyUI;
using Microsoft.Xna.Framework.Media;
using LD40.Interface;

namespace LD40.World
{
    public class Map
    {
        public enum MapState
        {
            UserControl,
            Animation, 
            EnemyTurns,
            Exit
        }

        double exitTimer = 0.0;

        public static DamagePopup damagePopup;
        public static readonly Point tileSize = new Point(16, 16);
        readonly Color selectedColor = Color.ForestGreen;
        readonly Color selectionColor = Color.LawnGreen;
        readonly Color enemyHoverColor = Color.Red;
        readonly Color attackEnemyColor = Color.DarkRed;
        readonly Color skillFriendTargetColor   = Color.DarkSlateBlue;
        readonly Color skillSelfTargetColor     = Color.CadetBlue;

        MapScene scene;
        public Random random = new Random();
        string levelName;
        Point mapSize;
        public Tile[,] tiles;
        List<Actor> units;
        List<Enemy> enemies;

        public int turnCounter = 1;

        Tileset tileset;
        Pathfinder pathfinder;
        Texture2D atlas;
        Tweener tweener;

        Actor selectedUnit = null;
        Sprite[] tileReachHighlight;
        Sprite tileMouseOverHighlight;
        Sprite unitHighlight;
        Sprite noCover100Icon;
        Sprite cover50Icon;
        Sprite[] blockableSprites;

        Sprite unitSprite;
        Sprite[] enemySprites;

        public MapState state = MapState.UserControl;
        float movementTime = 0.5f;


        public Map(string[] mapData, Texture2D atlas, ContentManager Content, MapScene scene)
        {            
            this.scene = scene;
            this.atlas = atlas;
            tileset = Tileset.tilesets["normal"];

            
            units = new List<Actor>();
            enemies = new List<Enemy>();
            tweener = new Tweener();
            
            Initialize(mapData);                        
        }

        public void Initialize(string[] mapData)
        {
            LoadSprites();
            pathfinder = new Pathfinder(null);

            levelName = mapData[0].Substring(1);

            mapSize = new Point(mapData[1].Length, mapData.Length - 1);
            tiles = new Tile[mapSize.X, mapSize.Y];

            for (int x = 0; x < mapSize.X; x++)
            {
                for (int y = 0; y < mapSize.Y; y++)
                {
                    char c = mapData[y + 1][x];
                    TileType type = TileType.Normal;
                    Blockable blockable = null;
                    if (c == ' ')
                        type = TileType.Empty;
                    else
                        type = TileType.Normal;


                    if(c >= '1' && c <= '3')
                    {
                        int blockableIndex = c - '1';                        
                        blockable = new Blockable(blockableSprites[blockableIndex], false);
                    }

                   tiles[x, y] = new Tile(x, y, type, blockable);

                    if(c == 's')
                    {
                        SpawnUnit(unitSprite, new Point(x,y));
                    }

                    if(c >= 'a' && c <= 'c')
                    {
                        EnemyType enem = (EnemyType)(c - 'a');
                        SpawnEnemy(enem, new Point(x, y));
                    }

                }
            }
            
            AutoTile();

            
            pathfinder.SetTiles(tiles);

        }

        private void AutoTile()
        {
            for (int x = 0; x < mapSize.X; x++)
            {
                for (int y = 0; y < mapSize.Y; y++)
                {
                    if (tiles[x, y].tileType == TileType.Empty)
                        continue;

                    int autoIndex = 0;
                    if (IsInRange(x, y - 1) && Tile(x, y - 1).tileType == Tile(x, y).tileType)
                        autoIndex += 1;
                    if (IsInRange(x + 1, y) && Tile(x + 1, y).tileType == Tile(x, y).tileType)
                        autoIndex += 2;
                    if (IsInRange(x, y + 1) && Tile(x, y + 1).tileType == Tile(x, y).tileType)
                        autoIndex += 4;
                    if (IsInRange(x - 1, y) && Tile(x - 1, y).tileType == Tile(x, y).tileType)
                        autoIndex += 8;
                    tiles[x, y].groundSprite = tileset.GetGroundTile(autoIndex);
                }
            }
        }

        void LoadSprites()
        {
            double[] length     = { 230, 200, 167, 167, 167, 167 };
            Rectangle[] rects   = { new Rectangle(0, 400, 16, 16),
                                    new Rectangle(16, 400, 16, 16),
                                    new Rectangle(32, 400, 16, 16),
                                    new Rectangle(48, 400, 16, 16),
                                    new Rectangle(32, 400, 16, 16),
                                    new Rectangle(16, 400, 16, 16),
                                  };
            Animation unitAnim  = new Animation(6, length, rects);
            unitSprite          = new Sprite(atlas, new Rectangle(0, 400, 16, 16), Color.White, unitAnim);

            enemySprites            = new Sprite[3];

            double[] enemLenght     = { 333, 167 };
            Rectangle[] enemRects   = { new Rectangle(0, 416, 16, 16), new Rectangle(16, 416, 16, 16) };
            Animation enemyAnim     = new Animation(2, enemLenght, enemRects);
            enemySprites[0]         = new Sprite(atlas, new Rectangle(0, 416, 16, 16), Color.White, enemyAnim);


            double[] enemLenght2 = { 333, 230, 333, 230 };
            Rectangle[] enemRects2 = {  new Rectangle(0, 432, 16, 16),
                                        new Rectangle(16, 432, 16, 16),
                                        new Rectangle(0, 432, 16, 16),
                                        new Rectangle(32, 432, 16, 16),
            };
            Animation enemyAnim2 = new Animation(4, enemLenght2, enemRects2);
            enemySprites[1] = new Sprite(atlas, new Rectangle(0, 432, 16, 16), Color.White, enemyAnim2);

            double[] enemLenght3 = { 333, 120, 250, 120 };
            Rectangle[] enemRects3 = {  new Rectangle(0, 448, 16, 16),
                                        new Rectangle(16, 448, 16, 16),
                                        new Rectangle(32, 448, 16, 16),
                                        new Rectangle(16, 448, 16, 16),
            };
            Animation enemyAnim3 = new Animation(4, enemLenght3, enemRects3);
            enemySprites[2] = new Sprite(atlas, new Rectangle(0, 448, 16, 16), Color.White, enemyAnim3);


            tileReachHighlight = new Sprite[16];
            XmlDocument td = new XmlDocument();
            td.Load("Content/tilesets.xml");
            foreach(XmlNode n in td.SelectNodes("tilesets/reachHighlight/ground/t"))
            {
                int index   = int.Parse(n.Attributes["auto"].Value);
                int x       = int.Parse(n.Attributes["x"].Value) * 16;
                int y       = int.Parse(n.Attributes["y"].Value) * 16;
                tileReachHighlight[index] = new Sprite(atlas, new Rectangle(x, y, 16, 16));
            }

            tileMouseOverHighlight = new Sprite(atlas, new Rectangle(320, 0, 16, 16));

            Animation unitHighlightAnim = new Animation(6, new double[] { 167, 167, 167, 167, 167, 167 }, 
                                                        new Rectangle[] {   new Rectangle(336, 0, 16, 16),
                                                                            new Rectangle(352, 0, 16, 16),
                                                                            new Rectangle(368, 0, 16, 16),
                                                                            new Rectangle(384, 0, 16, 16),
                                                                            new Rectangle(368, 0, 16, 16),
                                                                            new Rectangle(352, 0, 16, 16)});

            unitHighlight = new Sprite(atlas, new Rectangle(336,0,16,16), Color.White, unitHighlightAnim);

            blockableSprites = new Sprite[3];
            blockableSprites[0] = new Sprite(atlas, new Rectangle(12 * 16, 0, 16, 16), Color.White, drawOffset: new Point(0, -4));
            blockableSprites[1] = new Sprite(atlas, new Rectangle(13 * 16, 0, 16, 16), Color.White, drawOffset: new Point(0, -2));
            blockableSprites[2] = new Sprite(atlas, new Rectangle(14 * 16, 0, 16, 16), Color.White, drawOffset: new Point(0, -2));

            cover50Icon = new Sprite(atlas, new Rectangle(19*16,0,16,16), Color.White, drawOffset: new Point(0, -4));
            noCover100Icon = new Sprite(atlas, new Rectangle(18 * 16, 0, 16, 16), Color.White, drawOffset: new Point(0, -8));
        }

        #region Game stuff

        public void SpawnUnit(Sprite unitSprite, Point pos)
        {
            Actor a = new Actor(unitSprite, pos, UnitDeath, pathfinder.CalculateReach);
            tiles[pos.X, pos.Y].actorOnTop = a;
            units.Add(a);
        }

        public void UnitDeath(Actor u)
        {
            units.Remove(u);
            Tile(u.tilePosition).actorOnTop = null;
            Sounds.Play("unitDeath");
            if (selectedUnit == u)
                selectedUnit = null;

            if(units.Count == 0)
            {
                state = MapState.Exit;
                exitTimer = 0.0;                
            }
        }

        public void SpawnEnemy(EnemyType type, Point pos)
        {
            Enemy e = Enemy.CreateEnemy(type, enemySprites, pos, EnemyDeath, pathfinder.CalculateReach);
            Tile(pos).actorOnTop = e;
            enemies.Add(e);            
        }

        public void EnemyDeath(Enemy e)
        {
            enemies.Remove(e);
            Tile(e.tilePosition).actorOnTop = null;
            Sounds.Play("enemyDeath");
            if (enemies.Count == 0)
            {
                state = MapState.Exit;
                exitTimer = 0.0;
            }
        }

        #endregion

        public void Update(double deltaTime, Camera camera, Canvas canvas)
        {
            tweener.Update((float)deltaTime);
            if(state == MapState.UserControl)
            {
                if (!canvas.HandleInput())
                {
                    HandleKeyboardInput();
                    HandleMouseInput(camera);
                }
            }
            if(state == MapState.Exit)
            {
                exitTimer += deltaTime;
                if(exitTimer >= 1.5)
                {
                    if (enemies.Count == 0)
                        scene.MapFinished();
                    else
                        scene.MapFailed();
                }
            }
        }

        #region Input

        private void HandleKeyboardInput()
        {            
            if (Input.GetKeyUp(Keys.Enter))
            {
                EndTurn();
            }

            if (Input.GetKeyUp(Keys.Tab))
            {
                SelectNextUnit();                   
            }

            if(selectedUnit != null)
            {
                if (Input.GetKeyUp(Keys.D1))
                {
                    selectedUnit.SelectSkill(Skills.Movement);
                }
                if (Input.GetKeyUp(Keys.D2))
                {
                    selectedUnit.SelectSkill(Skills.Attacking);
                }
                if (Input.GetKeyUp(Keys.D3))
                {
                    selectedUnit.SelectSkill(Skills.Healing);
                }
                if (Input.GetKeyUp(Keys.D4))
                {
                    selectedUnit.SelectSkill(Skills.Reloading);
                }
            }
            

        }

        void SelectNextUnit()
        {
            if (state != MapState.UserControl)
                return;
            if (selectedUnit == null)
            {
                SelectUnit(units[0]);
                tweener.Tween(scene.GetCamera(), new { posX = selectedUnit.realPosition.X, posY = selectedUnit.realPosition.Y }, 0.5f);
                return;
            }
            for (int i = 0; i < units.Count; i++)
            {
                if(units[i] == selectedUnit)
                {
                    SelectUnit(units[(i + 1) % units.Count]);
                    tweener.Tween(scene.GetCamera(), new { posX = selectedUnit.realPosition.X, posY = selectedUnit.realPosition.Y }, 0.5f);
                    return;
                }
            }
        }

        Point lastMousePos;
        Point mousePos;

        private void HandleMouseInput(Camera camera)
        {

            lastMousePos = mousePos;
            mousePos = camera.ToWorld(Input.GetMousePosition()) / tileSize;

            if (state != MapState.UserControl)
                return;

            if (Input.GetLeftMouseDown() && IsInRange(mousePos))
            {
                Tile t = Tile(mousePos);
                if (t.actorOnTop != null && t.actorOnTop.faction == Faction.User)
                {
                    if (selectedUnit == t.actorOnTop && !IsTarget(selectedUnit.GetCurrentSkillTarget(), SkillTarget.Self))
                    {
                        UnselectUnit();
                        return;
                    }
                    else if(selectedUnit == null || !IsTarget(selectedUnit.GetCurrentSkillTarget(), SkillTarget.Friend))
                    {
                        SelectUnit(t.actorOnTop);
                        return;
                    }
                    
                }

                if(selectedUnit != null)
                {
                    Skills s            = selectedUnit.selectedSkill;
                    SkillTarget target  = selectedUnit.GetCurrentSkillTarget();
                    if (t.actorOnTop != null)
                    {

                        if (s == Skills.Healing && selectedUnit.skillReachable.Contains(mousePos))
                        {
                            if (t.actorOnTop == selectedUnit && IsTarget(target, SkillTarget.Self) || t.actorOnTop.faction == Faction.User && IsTarget(target, SkillTarget.Friend))
                            {
                                t.actorOnTop.TakeDamage(-selectedUnit.GetCurrentHealing());
                                Sounds.Play("healing");
                                Effects.ShowEffect("healing", t.actorOnTop.realPosition);
                                selectedUnit.UsedSkill();
                            }
                            
                        }        
                        else if(s == Skills.Attacking)
                        {
                            if (t.actorOnTop.faction == Faction.Enemy && selectedUnit.skillReachable.Contains(t.position))
                            {
                                //Implement cover and shot chance
                                Attack(selectedUnit, t.actorOnTop);                               
                            } else
                            {
                                UnselectUnit();
                            }
                        } else if(s == Skills.None ||s == Skills.Movement)
                        {
                            if (t.actorOnTop.faction == Faction.Enemy)
                            {
                                if (selectedUnit.skillReachable.Contains(t.position))
                                    selectedUnit.SelectSkill(Skills.Attacking);
                                else
                                    UnselectUnit();
                            }
                        }
                         
                    }
                    else if (s == Skills.Movement && selectedUnit.IsReachable(mousePos))
                    {
                        MoveUnit(selectedUnit, mousePos);
                        return;
                    }
                }
                
            }

            if (Input.GetRightMouseDown())
            {
                if(selectedUnit != null)
                {
                    if (selectedUnit.selectedSkill == Skills.Attacking || selectedUnit.selectedSkill == Skills.Healing)
                        selectedUnit.SelectSkill(Skills.Movement);
                    else
                        SelectUnit(null);
                }
            }
            
        }


        bool isDragging = false;

        public Vector2 CameraInput(double deltaTime, Camera camera)
        {
            Vector2 camMove = new Vector2();

            //zoom
            float oldZoom = camera.zoom;

            if (oldZoom > 2f && (Input.GetMouseWheelDelta() < 0 || Input.GetKeyUp(Keys.PageUp)))
            {
                tweener.Tween(camera, new { zoom = 2f }, 0.3f);                
            }
            else if (oldZoom < 4f && (Input.GetMouseWheelDelta() > 0 || Input.GetKeyUp(Keys.PageDown)))
            {
                tweener.Tween(camera, new { zoom = 4f }, 0.3f);
            }


            float camSpeed = 500f;

            if (Input.GetKeyPressed(Keys.D))
                camMove.X += camSpeed * (float)deltaTime;
            if (Input.GetKeyPressed(Keys.A))
                camMove.X -= camSpeed * (float)deltaTime;
            if (Input.GetKeyPressed(Keys.S))
                camMove.Y += camSpeed * (float)deltaTime;
            if (Input.GetKeyPressed(Keys.W))
                camMove.Y -= camSpeed * (float)deltaTime;

            if (Input.GetMiddleMouseDown())
                isDragging = true;

            if (Input.GetMiddleMouseUp())
                isDragging = false;

            //Drag axis invertable via Settings, -> *(-1) 
            if (isDragging)
                camMove += Input.GetMousePositionDelta().ToVector2();
            
            return camMove;
        }

        #endregion

        #region Render

        public void Draw(SpriteBatch spriteBatch)
        {

            HashSet<Point> moveableTiles = (selectedUnit == null) ? null : selectedUnit.GetReachableTiles();

            for (int x = 0; x < mapSize.X; x++)
            {
                for (int y = 0; y < mapSize.Y; y++)
                {
                    tiles[x, y].Render(spriteBatch);
                    if (moveableTiles != null && moveableTiles.Contains(new Point(x, y)))
                        DrawTileReachHighlight(spriteBatch, moveableTiles, x, y);
                }
            }

            Actor highlightSelectionUnit = null;            

            if (IsInRange(mousePos))
            {
                tileMouseOverHighlight.Render(spriteBatch, mousePos * tileSize);

                if (Tile(mousePos).actorOnTop != null && Tile(mousePos).actorOnTop != selectedUnit)
                {
                    highlightSelectionUnit = Tile(mousePos).actorOnTop;                    
                }
            }

            HashSet<Point> skillReachable = (selectedUnit == null) ? null : selectedUnit.GetSkillReachableTiles();
            SkillTarget target = (selectedUnit == null) ? SkillTarget.None : selectedUnit.GetCurrentSkillTarget();
            foreach (Actor u in units)
            {
                if (skillReachable != null && IsTarget(target, SkillTarget.Friend) && skillReachable.Contains(u.tilePosition))
                {
                    if (u == selectedUnit)
                        unitHighlight.Render(spriteBatch, u.realPosition, skillSelfTargetColor);
                    else
                        unitHighlight.Render(spriteBatch, u.realPosition, skillFriendTargetColor);
                }
                else if (u == selectedUnit)
                    unitHighlight.Render(spriteBatch, u.realPosition, selectedColor);
                else if (u == highlightSelectionUnit)
                    unitHighlight.Render(spriteBatch, u.realPosition, selectionColor);
                
                u.Render(spriteBatch);
            }

            foreach (Enemy e in enemies)
            {
                if(skillReachable != null && IsTarget(target, SkillTarget.Enemy) && skillReachable.Contains(e.tilePosition))
                    unitHighlight.Render(spriteBatch, e.realPosition, attackEnemyColor);
                else if (e == highlightSelectionUnit)
                    unitHighlight.Render(spriteBatch, e.realPosition, enemyHoverColor);
                e.Render(spriteBatch);
            }

            if(selectedUnit != null && selectedUnit.selectedSkill == Skills.Attacking && 
                IsInRange(mousePos) && Tile(mousePos).actorOnTop != null && 
                Tile(mousePos).actorOnTop.faction == Faction.Enemy &&
                selectedUnit.skillReachable.Contains(mousePos))
            {
                if(!HighlightCover(spriteBatch, Tile(mousePos).actorOnTop, selectedUnit))
                {
                    noCover100Icon.Render(spriteBatch, Tile(mousePos).actorOnTop.realPosition);
                }
                
            }        

        }

        bool HighlightCover(SpriteBatch spriteBatch, Actor attacked, Actor attacker)
        {
            Point from = attacker.tilePosition;
            Point to = attacked.tilePosition;

            Vector2 f = from.ToVector2();
            Vector2 t = to.ToVector2();

            double angle = Math.Atan2(f.Y - t.Y, f.X - t.X);
            if (angle < 0)
                angle += 2 * Math.PI;
            double degree = angle * 180.0 / Math.PI + 90;

            if (degree >= 315.0 && degree <= 45.0)
            {
                //check Up tile
                if (IsInRange(to.X, to.Y - 1) && Tile(to.X, to.Y - 1).HasCover())
                {
                    cover50Icon.Render(spriteBatch, new Point(to.X, to.Y - 1) * tileSize);
                    return true;
                }
            }
            else if (degree >= 45.0 && degree <= 135.0)
            {
                //check right
                if(IsInRange(to.X + 1, to.Y) && Tile(to.X + 1, to.Y).HasCover())
                {
                    cover50Icon.Render(spriteBatch, new Point(to.X + 1, to.Y) * tileSize);
                    return true;
                }
            }
            else if (degree >= 135.0 && degree <= 225.0)
            {
                //check down
                if(IsInRange(to.X, to.Y + 1) && Tile(to.X, to.Y + 1).HasCover())
                {
                    cover50Icon.Render(spriteBatch, new Point(to.X, to.Y + 1) * tileSize);
                    return true;
                }
            }
            else if (degree >= 225.0 && degree <= 315.0)
            {
                //check left
                if(IsInRange(to.X - 1, to.Y) && Tile(to.X - 1, to.Y).HasCover())
                {
                    cover50Icon.Render(spriteBatch, new Point(to.X - 1, to.Y) * tileSize);
                    return true;
                }
            }

            return false;
        }

        void DrawTileReachHighlight(SpriteBatch spriteBatch, HashSet<Point> moveableTiles, int x, int y)
        {

            int autoIndex = 0;
            if (IsInRange(x, y - 1) && moveableTiles.Contains(new Point(x,y - 1)))
                autoIndex += 1;
            if (IsInRange(x + 1, y) && moveableTiles.Contains(new Point(x + 1, y)))
                autoIndex += 2;
            if (IsInRange(x, y + 1) && moveableTiles.Contains(new Point(x, y + 1)))
                autoIndex += 4;
            if (IsInRange(x - 1, y) && moveableTiles.Contains(new Point(x - 1, y)))
                autoIndex += 8;                        

            tileReachHighlight[autoIndex].Render(spriteBatch, new Point(x, y) * tileSize);            
        }

        #endregion

        #region game logic

        public void Attack(Actor attacker, Actor attacked)
        {
            Debug.Assert(attacker.skillReachable.Contains(attacked.tilePosition));

            bool hasCover = IsInCoverTo(attacked, attacker);

            if(!hasCover || random.NextDouble() >= 0.5)
            {
                //hit
                attacked.TakeDamage(attacker.GetCurrentDamage());
                attacker.currAmmo--;
                attacker.UsedSkill();
            }
            else
            {
                //no hit
                attacker.currAmmo--;
                attacker.UsedSkill();
                damagePopup.Show(attacker,"miss");
            }


            if (attacker.faction == Faction.User)
            {
                Sounds.Play("playerAttack");
                Effects.ShowEffect("playerAttack", attacker.realPosition);
            } else
            {
                Sounds.Play("enemyAttack");
                Effects.ShowEffect("redLaser", attacker.realPosition);
            }
        }

        public bool IsInCoverTo(Actor attacked, Actor attacker)
        {
            Point from = attacker.tilePosition;
            Point to = attacked.tilePosition;
            
            Vector2 f = from.ToVector2();
            Vector2 t = to.ToVector2();                                    

            double angle = Math.Atan2(f.Y - t.Y, f.X - t.X);
            if (angle < 0)
                angle += 2 * Math.PI;
            double degree = angle * 180.0 / Math.PI + 90;
            
            if(degree >= 315.0 && degree <= 45.0)
            {
                //check Up tile
                return IsInRange(to.X, to.Y - 1) && Tile(to.X, to.Y - 1).HasCover();
            } else if(degree >= 45.0 && degree <= 135.0)
            {
                //check right
                return IsInRange(to.X + 1, to.Y) && Tile(to.X + 1, to.Y).HasCover();
            }
            else if (degree >= 135.0 && degree <= 225.0)
            {
                //check down
                return IsInRange(to.X, to.Y + 1) && Tile(to.X, to.Y + 1).HasCover();
            }
            else if (degree >= 225.0 && degree <= 315.0)
            {
                //check left
                return IsInRange(to.X - 1, to.Y) && Tile(to.X - 1, to.Y).HasCover();
            }        

            return false;
        }

        private void MoveUnit(Actor a, Point target)
        {
            state = MapState.Animation;
            Point realTarget = target * tileSize;
            Tile(a.tilePosition).actorOnTop = null;
            Tile(target).actorOnTop = a;

            float dist = (target - a.tilePosition).ToVector2().Length();
            float movementTime = this.movementTime + 0.25f * (dist-1);
            tweener.Tween(a, new { X = realTarget.X, Y = realTarget.Y }, movementTime).Ease(Ease.SineInOut).OnComplete(() => { MovementFinished(a, target); });
            Action rev = () => { tweener.Tween(a, new { yOffset = 0 }, movementTime / 2f).Ease(Ease.QuadOut); };
            tweener.Tween(a, new { yOffset = (int)8*movementTime }, movementTime / 2f).Ease(Ease.QuadOut).OnComplete(rev);
            a.reachableTiles = null;
            Sounds.Play("jumpStart");
            Effects.ShowEffect("jumpStart", a.realPosition);
        }

        private void MovementFinished(Actor a, Point target)
        {
            state = MapState.UserControl;
            a.MoveFinished();
            a.tilePosition = target;
            Sounds.Play("jumpLand");
            Effects.ShowEffect("jumpLand", a.realPosition);
        }

        private void SelectUnit(Actor a)
        {
            if (a == null)
            {
                UnselectUnit();
                return;
            }

            Debug.Assert(a.faction == Faction.User);
            Sounds.Play("unitSelect");
            selectedUnit = a;
            if (a.skillReachable == null)
                a.skillReachable = pathfinder.CalculateReach(a.tilePosition, a.GetSkillRange(), true);
            a.SelectSkill(a.movesLeft > 0 ? Skills.Movement : Skills.None);                                        
            //update ui and stuff
        }

        private void UnselectUnit()
        {
            Sounds.Play("unitUnselect");
            selectedUnit = null;
        }

        public void EndTurn()
        {
            UnselectUnit();            
            MakeFirstEnemyTurn();            
        }

        private void StartNewTurn()
        {            
            foreach (Actor u in units)
            {
                u.EndTurn();
            }
            foreach(Enemy e in enemies)
            {
                e.EndTurn();
            }

            turnCounter++;
            state = MapState.UserControl;
            Sounds.Play("newTurn");
        }

        public void MakeFirstEnemyTurn()
        {
           
            state = MapState.EnemyTurns;
            Debug.Assert(enemies.Count > 0);
            Enemy e = enemies[0];
            MakeEnemyTurn(e, () => MakeNextEnemyTurn(1));
        }

        public void MakeNextEnemyTurn(int index)
        {
            if (index >= enemies.Count)
                StartNewTurn();
            else if (index == enemies.Count - 1)
                MakeEnemyTurn(enemies[index], StartNewTurn);
            else
                MakeEnemyTurn(enemies[index], () => MakeNextEnemyTurn(index + 1));
        }

        public void MakeEnemyTurn(Enemy e, Action OnFinish)
        {
            Action doit;
            Point targetTile = e.MakeTurn(this, out doit);
            if(targetTile != e.tilePosition)
            {
                Debug.Assert(Tile(targetTile).IsWalkable());                
                Tile(e.tilePosition).actorOnTop = null;
                Tile(targetTile).actorOnTop = e;
                e.tilePosition = targetTile;
                tweener.Tween(e, new { X = targetTile.X * 16, Y = targetTile.Y * 16 }, 1f).OnComplete(doit).OnComplete(OnFinish);
                Effects.ShowEffect("jumpLand", e.realPosition);
            }
            else
            {
                doit();
                OnFinish();
            }
        }

        public string GetUnitInfoString()
        {            
            return selectedUnit != null ? selectedUnit.GetInfoString() : "";
        }

        public Actor GetSelectedUnit()
        {
            return selectedUnit;
        }

        public Enemy GetMouseOverEnemy()
        {
            if (IsInRange(mousePos) && Tile(mousePos).actorOnTop != null && Tile(mousePos).actorOnTop.faction == Faction.Enemy)            
                return (Enemy)Tile(mousePos).actorOnTop;
            else
                return null;
        }

        #endregion

        #region Helper

        public bool IsTarget(SkillTarget given, SkillTarget wanted)
        {
            return (given & wanted) > 0;
        }

        private Tile Tile(int X, int Y)
        {
            return tiles[X, Y];
        }

        private Tile Tile(Point p)
        {
            return tiles[p.X, p.Y];
        }

        public bool IsInRange(int x, int y)
        {
            return x >= 0 && y >= 0 && x < mapSize.X && y < mapSize.Y;
        }

        public bool IsInRange(Point p)
        {
            return IsInRange(p.X, p.Y);
        }

        public IEnumerable<Point> IterateNeighboursFourDir(Point p)
        {
            return IterateNeighboursFourDir(p.X, p.Y);
        }

        public IEnumerable<Point> IterateNeighboursFourDir(int x, int y)
        {
            if (IsInRange(x - 1, y))
                yield return new Point(x - 1, y);
            if (IsInRange(x + 1, y))
                yield return new Point(x + 1, y);
            if (IsInRange(x, y - 1))
                yield return new Point(x, y - 1);
            if (IsInRange(x, y + 1))
                yield return new Point(x, y + 1);
        }

        private IEnumerable<Point> IterateNeighboursEightDir(int x, int y)
        {
            for (int xx = x - 1; xx <= x + 1; xx++)
            {
                for (int yy = y - 1; yy <= y + 1; yy++)
                {
                    if (IsInRange(xx, yy) && (xx != x || yy != y))
                    {
                        yield return new Point(xx, yy);
                    }
                }
            }
        }        

        private IEnumerable<Point> IterateCornerNeighbours(int x, int y)
        {
            if (IsInRange(x - 1, y - 1))
                yield return new Point(x - 1, y - 1);

            if (IsInRange(x - 1, y + 1))
                yield return new Point(x - 1, y + 1);

            if (IsInRange(x + 1, y + 1))
                yield return new Point(x + 1, y + 1);

            if (IsInRange(x + 1, y - 1))
                yield return new Point(x + 1, y - 1);
        }

        #endregion

    }
}
