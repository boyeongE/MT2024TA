using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class carmove : MonoBehaviour

{
    public float rotationSpeed = 10f;
    public float moveSpeed = 5.0f; // �̵� �ӵ�
    private CharacterController controller; // CharacterController ������Ʈ
    private Vector3 moveDirection; // �̵� ����

   




    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<CharacterController>();
       


    }

    // Update is called once per frame
    void Update()
    {
        float horizontalInput = Input.GetAxis("Horizontal"); // ���� ���� �̵� �Է� ���� (��,��)
        float verticalInput = Input.GetAxis("Vertical"); // ���� ���� �̵� �Է� ���� (��, ��)

        // �̵� ���� ����
        Vector3 move = transform.TransformDirection(new Vector3(horizontalInput, 0, verticalInput));
        moveDirection = move * moveSpeed;

 

        // �̵� ����
        controller.Move(moveDirection * Time.deltaTime);
        
        transform.Rotate(new Vector3 (0, horizontalInput * rotationSpeed * Time.deltaTime, 0));

    }
}
