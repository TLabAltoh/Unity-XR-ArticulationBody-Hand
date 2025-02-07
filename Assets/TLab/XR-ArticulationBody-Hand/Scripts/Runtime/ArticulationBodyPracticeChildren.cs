using UnityEngine;

namespace TLab.XR.ArticulationBodyHand
{
    public class ArticulationBodyPracticeChildren : MonoBehaviour
    {
        private ArticulationBody m_articulationBody;

        void Start()
        {
            m_articulationBody = GetComponent<ArticulationBody>();
        }

        void Update()
        {
            if (Input.GetKey(KeyCode.W))
            {
                var xDrive = m_articulationBody.xDrive;
                xDrive.target -= 200f * Time.deltaTime;
                xDrive.target = Mathf.Clamp(xDrive.target, xDrive.lowerLimit, xDrive.upperLimit);
                m_articulationBody.xDrive = xDrive;
            }
            else if (Input.GetKey(KeyCode.S))
            {
                var xDrive = m_articulationBody.xDrive;
                xDrive.target += 200f * Time.deltaTime;
                xDrive.target = Mathf.Clamp(xDrive.target, xDrive.lowerLimit, xDrive.upperLimit);
                m_articulationBody.xDrive = xDrive;
            }
        }
    }
}
