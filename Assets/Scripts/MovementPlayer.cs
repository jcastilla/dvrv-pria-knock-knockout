using UnityEngine;
using Unity.Netcode;
using System.Collections;

public class MovementPlayer : NetworkBehaviour
{
    [Header("Movimiento")]
    public float moveForce = 5f;
    public float maxSpeed = 50f;

    private Rigidbody rb;
    private Renderer render;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsServer)
        {
            Vector3 randomPos = new Vector3(
                Random.Range(-5f, 5f),
                2f,
                Random.Range(-5f, 5f)
            );

            transform.position = randomPos;
        }

        ChangeColorRandom();
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        render = GetComponent<Renderer>();
    }

    void FixedUpdate()
    {
        if(!IsOwner) return;

        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        Vector3 direction = new Vector3(h, 0, v);

        if (direction.sqrMagnitude > 0.01f)
            MoveRequestRpc(direction);
    }

    [Rpc(SendTo.Server)]
    private void MoveRequestRpc(Vector3 direction)
    {
        rb.AddForce(direction * moveForce, ForceMode.Force);

        if (rb.linearVelocity.magnitude > maxSpeed)
            rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;

        UpdatePositionClientRpc(transform.position);
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void UpdatePositionClientRpc(Vector3 pos)
    {
        if (IsServer) return;

        transform.position = pos;
    }

    private void ChangeColorRandom()
    {
        if (render != null)
            render.material.color = new Color(Random.value, Random.value, Random.value);
    }
}