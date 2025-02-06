using UnityEngine;

namespace TLab.XR.ArticulationBodyHand
{
    public class ArticulationBodyPractice : MonoBehaviour
    {
        private ArticulationBody m_articulationBody;

        private Vector3 m_position;
        private Quaternion m_rotation;

        void Start()
        {
            m_articulationBody = GetComponent<ArticulationBody>();
            m_position = transform.position;
            m_rotation = transform.rotation;
        }

        void Update()
        {
            if (Input.GetKey(KeyCode.RightArrow))
                m_position.x += 10f * Time.deltaTime;
            else if (Input.GetKey(KeyCode.LeftArrow))
                m_position.x -= 10f * Time.deltaTime;

            var delta = m_position - transform.position;
            var speed = delta / Time.fixedDeltaTime;
            m_articulationBody.velocity = speed;
            m_articulationBody.angularVelocity = Vector3.zero;
        }
    }
}
