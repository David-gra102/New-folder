using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;
using Unity.Netcode;

public class BombController : NetworkBehaviour
{
    [Header("Bomb")]
    public GameObject bombPrefeb;
    public KeyCode inputKey = KeyCode.Space;
    public float bombFuseTime = 3f;
    public int bombAmmount = 1;
    private int bombsRemaining;

    [Header ("Explosion")]
    public Explosion explosionPrefab;
    public LayerMask explosionLayerMask;
    public float explosionDuration = 1f;
    public int explosionRadius = 1;

    [Header("Destructable")]
    public Tilemap destructableTiles;
    public Destructable destructiblePrefab;

    private void OnStart()
    {
        destructableTiles = FindObjectOfType<Tilemap>();
    }


    private void OnEnable() 
    {
        bombsRemaining = bombAmmount;
    }

    private void Update() 
    {
        if(!IsOwner)
        {
            return;
        }
        if (IsLocalPlayer && bombsRemaining > 0 && Input.GetKeyDown(inputKey)) 
        {

            //StartCoroutine(PlaceBomb());
            PlaceBombServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void PlaceBombServerRpc()
    {
        StartCoroutine(PlaceBomb());
    }


    private IEnumerator PlaceBomb() 
    {
        Vector2 position = transform.position;
        position.x = Mathf.Round(position.x);
        position.y = Mathf.Round(position.y);

        GameObject bomb = Instantiate(bombPrefeb, position, Quaternion.identity);
        bombsRemaining--;

        yield return new WaitForSeconds(bombFuseTime);

        //...BOOM!
        position = bomb.transform.position;
        position.x = Mathf.Round(position.x);
        position.y = Mathf.Round(position.y);

        Explosion explosion = Instantiate(explosionPrefab, position, Quaternion.identity);
        explosion.SetActiveRenderer(explosion.start);
        explosion.DestroyAfter(explosionDuration);

        Explode(position, Vector2.up, explosionRadius);
        Explode(position, Vector2.down, explosionRadius);
        Explode(position, Vector2.left, explosionRadius);
        Explode(position, Vector2.right, explosionRadius);

        Destroy(bomb);
        bombsRemaining++;
    }

    private void Explode(Vector2 position, Vector2 direction, int length) 
    {
        if (length <= 0) 
        {
            return;
        }

        position += direction;

        if(Physics2D.OverlapBox(position, Vector2.one /2, 0f, explosionLayerMask))
        {
            ClearDestructable(position);
            return;
        }

        Explosion explosion = Instantiate(explosionPrefab, position, Quaternion.identity);
        explosion.SetActiveRenderer(length>1 ? explosion.middle : explosion.end);
        explosion.SetDirection(direction);
        explosion.DestroyAfter(explosionDuration);

        Explode(position, direction, length - 1);
    }

    private void ClearDestructable(Vector2 position) 
    {
        Vector3Int cell = destructableTiles.WorldToCell(position);
        TileBase tile = destructableTiles.GetTile(cell);

        if (tile != null) 
        {
            Instantiate(destructiblePrefab, position, Quaternion.identity);
            destructableTiles.SetTile(cell, null);
        }
    }

    private void OnTriggerExit2D(Collider2D other) 
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Bomb")) 
        {
            other.isTrigger = false;
        }
    }

    public void AddBomb()
    {
        bombAmmount++;
        bombsRemaining++;
    }

}
