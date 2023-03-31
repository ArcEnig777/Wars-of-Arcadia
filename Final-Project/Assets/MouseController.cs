
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using static ArrowTranslator;

public class MouseController : MonoBehaviour
{
        public GameObject cursor;
        public float speed;
        public GameObject characterPrefab;
        public PlayerController character;
        public EnemyController target;

        private PathFinder pathFinder;
        private RangeFinder rangeFinder;
        private ArrowTranslator arrowTranslator;
        public List<OverlayTile> path;
        public List<OverlayTile> rangeFinderTiles;
        public List<OverlayTile> attackRangeFinderTiles;
        public bool isMoving;
        public bool isAttacking;

        private void Start()
        {
            pathFinder = new PathFinder();
            rangeFinder = new RangeFinder();
            arrowTranslator = new ArrowTranslator();

            path = new List<OverlayTile>();
            isMoving = false;
            isAttacking = false;
            rangeFinderTiles = new List<OverlayTile>();
            attackRangeFinderTiles = new List<OverlayTile>();
        }

        void LateUpdate()
        {
            RaycastHit2D? hit = GetFocusedOnTile();
            /*if (character.standingOnTile == null)
            {
                    RaycastHit2D? player = GetFocusedOnPlayer(character);
                    OverlayTile ptile = player.Value.collider.gameObject.GetComponent<OverlayTile>();
                    PositionCharacterOnLine(ptile);
                    character.standingOnTile = ptile;
            }*/

            if (hit.HasValue)
            {
                OverlayTile tile = hit.Value.collider.gameObject.GetComponent<OverlayTile>();
                cursor.transform.position = tile.transform.position;
                cursor.gameObject.GetComponent<SpriteRenderer>().sortingOrder = tile.transform.GetComponent<SpriteRenderer>().sortingOrder;

                if (rangeFinderTiles.Contains(tile) && !isMoving && !isAttacking && character != null)
                {
                    path = pathFinder.FindPath(character.standingOnTile, tile, rangeFinderTiles);

                    foreach (var item in rangeFinderTiles)
                    {
                        MapManager.Instance.map[item.grid2DLocation].SetSprite(ArrowDirection.None);
                    }

                    for (int i = 0; i < path.Count; i++)
                    {
                        var previousTile = i > 0 ? path[i - 1] : character.standingOnTile;
                        var futureTile = i < path.Count - 1 ? path[i + 1] : null;

                        var arrow = arrowTranslator.TranslateDirection(previousTile, path[i], futureTile);
                        path[i].SetSprite(arrow);
                    }
                }

                if (Input.GetMouseButtonDown(0) && !isAttacking)
                {
                    tile.gameObject.GetComponent<SpriteRenderer>().color = new Color(0, 0, 0.75f, 0.5f);

                    RaycastHit2D? unit = GetFocusedOnCharacter();

                    if(unit.HasValue)
                    {

                        Debug.Log(unit.Value);

                        if (character != null)
                        {
                            if(unit.Value.collider.gameObject.GetComponent<PlayerController>().standingOnTile == character.standingOnTile)
                            {
                                GetInRangeAtkTiles();
                                isAttacking = true;
                            }
                            else
                            {
                                character = unit.Value.collider.gameObject.GetComponent<PlayerController>();
                                RaycastHit2D? player = GetFocusedOnPlayer(character);
                                OverlayTile ptile = player.Value.collider.gameObject.GetComponent<OverlayTile>();
                                PositionCharacterOnLine(ptile);
                                character.standingOnTile = ptile;
                                GetInRangeTiles();
                            }
                        }
                        else
                        {
                            character = unit.Value.collider.gameObject.GetComponent<PlayerController>();

                            Debug.Log(character);

                            RaycastHit2D? player = GetFocusedOnPlayer(character);
                            OverlayTile ptile = player.Value.collider.gameObject.GetComponent<OverlayTile>();
                            PositionCharacterOnLine(ptile);
                            character.standingOnTile = ptile;
                            GetInRangeTiles();
                        }

                    }

                    /*if (character == null)
                    {
                        RaycastHit2D? unit = GetFocusedOnCharacter();

                        Debug.Log(unit.Value);

                        character = unit.Value.collider.gameObject.GetComponent<PlayerController>();

                        Debug.Log(character);

                        RaycastHit2D? player = GetFocusedOnPlayer(character);
                        OverlayTile ptile = player.Value.collider.gameObject.GetComponent<OverlayTile>();
                        PositionCharacterOnLine(ptile);
                        character.standingOnTile = ptile;
                    }*/
                    else if(character != null)
                    {
                        if (character.standingOnTile == null)
                        {
                            RaycastHit2D? player = GetFocusedOnPlayer(character);
                            OverlayTile ptile = player.Value.collider.gameObject.GetComponent<OverlayTile>();
                            PositionCharacterOnLine(ptile);
                            character.standingOnTile = ptile;
                        } 
                        else if (path.Count > 0)
                        {
                            isMoving = true;
                            foreach (var item in rangeFinderTiles)
                            {
                                MapManager.Instance.map[item.grid2DLocation].SetSprite(ArrowDirection.None);
                            }
                            tile.gameObject.GetComponent<OverlayTile>().HideTile();
                        }
                        else
                        {
                            character = null;
                        }
                    }

                }
                else if (Input.GetMouseButtonDown(1))
                {

                    foreach (var item in rangeFinderTiles)
                    {
                        MapManager.Instance.map[item.grid2DLocation].SetSprite(ArrowDirection.None);
                    }
                    character = null;
                    isAttacking = false;

                }
                else if (Input.GetMouseButtonDown(0) && isAttacking)
                {
                    RaycastHit2D? ehit = GetFocusedOnTile();

                    if (ehit.HasValue)
                    {
                        OverlayTile etile = hit.Value.collider.gameObject.GetComponent<OverlayTile>();

                        if(attackRangeFinderTiles.Contains(etile) && tile.isPlayerBlocked)
                        {
                            Debug.Log("I found an enemy!");
                            RaycastHit2D? enemy = GetFocusedOnEnemy();

                            if (enemy.HasValue)
                            {
                                target = enemy.Value.collider.gameObject.GetComponent<EnemyController>();

                                target.takeDamage(character.getAttack() - target.getDefense());
                                
                                if (target != null)
                                {
                                    character.takeDamage(target.getAttack() - character.getDefense());
                                }
                                
                            }
                        }
                    }

                    isAttacking = false;
                    character = null;
                    target = null;
                }
            }

            if(isMoving)
            {
                MoveAlongPath();
            }

        }

        private void MoveAlongPath()
        {
            var step = speed * Time.deltaTime;

            float zIndex = path[0].transform.position.z;
            character.transform.position = Vector2.MoveTowards(character.transform.position, path[0].transform.position, step);
            character.transform.position = new Vector3(character.transform.position.x, character.transform.position.y, zIndex);

            if(Vector2.Distance(character.transform.position, path[0].transform.position) < 0.00001f)
            {
                PositionCharacterOnLine(path[0]);
                path.RemoveAt(0);
            }
            if(path.Count <= 0)
            {
                isMoving = false;
                isAttacking = true;
                GetInRangeAtkTiles();
            }

        }

        private void PositionCharacterOnLine(OverlayTile tile)
        {
            character.transform.position = new Vector3(tile.transform.position.x, tile.transform.position.y, tile.transform.position.z);
            character.GetComponent<SpriteRenderer>().sortingOrder = tile.GetComponent<SpriteRenderer>().sortingOrder;
            character.standingOnTile = tile;
        }

        private static RaycastHit2D? GetFocusedOnTile()
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 mousePos2D = new Vector2(mousePos.x, mousePos.y);

             LayerMask layerT = LayerMask.GetMask("Solidtitles");

            RaycastHit2D[] hits = Physics2D.RaycastAll(mousePos2D, Vector2.zero, 20.0f, layerT);

            if (hits.Length > 0)
            {
                return hits.OrderByDescending(i => i.collider.transform.position.z).First();
            }

            return null;
        }

        private static RaycastHit2D? GetFocusedOnCharacter()
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 mousePos2D = new Vector2(mousePos.x, mousePos.y);

            LayerMask layer = LayerMask.GetMask("Units");

            RaycastHit2D[] pUnit = Physics2D.RaycastAll(mousePos2D, Vector2.zero, 20.0f, layer);

            if (pUnit.Length > 0)
            {
                Debug.Log("I found something interesting");
                return pUnit.OrderByDescending(i => i.collider.transform.position.z).First();
            }

            return null;
        }

        private static RaycastHit2D? GetFocusedOnEnemy()
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 mousePos2D = new Vector2(mousePos.x, mousePos.y);

            LayerMask layer = LayerMask.GetMask("Enemy");

            RaycastHit2D[] pUnit = Physics2D.RaycastAll(mousePos2D, Vector2.zero, 20.0f, layer);

            if (pUnit.Length > 0)
            {
                Debug.Log("I found something interesting");
                return pUnit.OrderByDescending(i => i.collider.transform.position.z).First();
            }

            return null;
        }

        private static RaycastHit2D? GetFocusedOnPlayer(PlayerController c)
        {
            Vector2 char2D = new Vector2(c.transform.position.x, c.transform.position.y);

            LayerMask layerT = LayerMask.GetMask("Solidtitles");

            RaycastHit2D[] hits = Physics2D.RaycastAll(char2D, Vector2.zero, 20.0f, layerT);

            if (hits.Length > 0)
            {
                return hits.OrderByDescending(i => i.collider.transform.position.z).First();
            }

            return null;
        }
        private void GetInRangeTiles()
        {
            rangeFinderTiles = rangeFinder.GetTilesInRange(new Vector2Int(character.standingOnTile.gridLocation.x, character.standingOnTile.gridLocation.y), 3);

            foreach (var item in rangeFinderTiles)
            {   
                    item.ShowTile();   
            }
        }
        private void GetInRangeAtkTiles()
        {
            attackRangeFinderTiles = rangeFinder.GetTilesInAtkRange(new Vector2Int(character.standingOnTile.gridLocation.x, character.standingOnTile.gridLocation.y), 1);

            foreach (var item in attackRangeFinderTiles)
            {   
                    item.ShowTileEnemy();   
            }
        }

   
}
