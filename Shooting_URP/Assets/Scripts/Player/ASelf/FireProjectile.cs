using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AttackType
{
    Default, Arrow, Double, Shot
}
public enum UpgradeType
{
    Damage, PenetrateCount, AttackDelay, BurstCount, MultiCount, SpreadAngle
}

public class FireProjectile : MonoBehaviour
{
    private List<GameObject> pools;

    private Level m_Level;
    private DrawPolygon m_DrawPolygon;
    private AudioSource m_AudioSource;
    private Transform m_ProjectileBundle;

    public AttackType m_AttackType;

    public GameObject m_Projectile;
    public WeaponPos m_WeaponPos;

    [Serializable]
    public struct BulletStruct
    {
        public String m_Type;

        [Space]
        [Header("무기")]
        public Transform[] m_Weapon;
        [Header("멀티건 버스트")]
        public float m_MultiBurstCount;

        [Space]
        [Header("공격력")]
        public float m_Damage;
        [Header("관통")]
        public int m_PenetrateCount;
        [Header("투사체속도")]
        public float m_ProjectileSpeed;
        [Header("투사체회전")]
        public float m_ProjectileRotate;

        [Space]
        [Header("공격속도")]
        public float m_AttackDelay;
        [Header("버스트")]
        public int m_BurstCount;
        [Header("버스트속도")]
        public float m_BurstSpeed;
        [Header("멀티샷")]
        public int m_MultiCount;
        [Header("탄퍼짐")]
        public float m_SpreadAngle;

        [Header("시작 포지션")]
        public float m_RightPos;

        [Space]
        [Header("오디오")]
        public AudioClip m_AudioClip;
        public float m_Volume;
    }
    public BulletStruct[] m_BulletStruct = new BulletStruct[1];
    public BulletStruct[] m_OriginStruct;

    [Serializable]
    public struct DetailUpgradeStruct
    {
        public UpgradeType m_UpgradeType;
        public float m_Value;
        public int m_DelayCount;
    }
    [Serializable]
    public struct UpgradeStruct
    {
        public String m_Type;
        public DetailUpgradeStruct[] m_DetailUpgrade;
    }
    public UpgradeStruct[] m_UpgradeStruct = new UpgradeStruct[1];

    public int m_WeaponLevel = 0;
    private int[] upgradeCount = new int[5];
    public int typeCount;

    private Rotatement[] m_Rotatement = new Rotatement[10];
    private float[] sin = new float[5];
    private float[] absolute = new float[5];
    private Vector3[] weaponPos = new Vector3[5];
    private float attackDelay;


    private void Awake()
    {
        m_OriginStruct = new BulletStruct[m_BulletStruct.Length];
        for(int i = 0; i < m_BulletStruct.Length; i++)
        {
            m_OriginStruct[i] = m_BulletStruct[i];
        }
    }

    private void Start()
    {
        m_Level = transform.parent.GetComponentInChildren<Level>();
        m_DrawPolygon = transform.parent.GetComponentInChildren<DrawPolygon>();
        m_AudioSource = GetComponent<AudioSource>();
        m_ProjectileBundle = GameObject.FindGameObjectWithTag("ProjectileBundle").transform;
        pools = new List<GameObject>();

        SetWeapon(m_AttackType);
    }

    public void SetWeapon(AttackType type)
    {
        int typeint = 0;
        for (int i = 0; i < m_BulletStruct.Length; i++)
        {
            if (m_BulletStruct[i].m_Type == type.ToString())
            {
                typeint = i;
            }
        }

        if (typeint != 0 && typeint == typeCount)
        {
            var detail = m_UpgradeStruct[typeCount].m_DetailUpgrade;
            for(int i = 0; i < detail.Length; i++)
            {
                if (upgradeCount[i] > 0)
                {
                    upgradeCount[i]--;
                }
                else
                {
                    upgradeCount[i] = 100;
                    switch (detail[i].m_UpgradeType.ToString())
                    {
                        case "Damage":
                            m_BulletStruct[typeCount].m_Damage += detail[i].m_Value;
                            break;
                        case "PenetrateCount":
                            m_BulletStruct[typeCount].m_PenetrateCount += (int)detail[i].m_Value;
                            break;
                        case "AttackDelay":
                            m_BulletStruct[typeCount].m_AttackDelay += detail[i].m_Value;
                            break;
                        case "BurstCount":
                            m_BulletStruct[typeCount].m_BurstCount += (int)detail[i].m_Value;
                            break;
                        case "MultiCount":
                            m_BulletStruct[typeCount].m_MultiCount += (int)detail[i].m_Value;
                            break;
                        case "SpreadAngle":
                            m_BulletStruct[typeCount].m_SpreadAngle += detail[i].m_Value;
                            break;
                    }
                }
            }
            m_WeaponLevel++;
        }
        else
        {
            m_WeaponLevel = 0;

            for (int i = 0; i < m_BulletStruct.Length; i++)
            {
                for (int j = 0; j < m_BulletStruct[i].m_Weapon.Length; j++)
                {
                    m_BulletStruct[i] = m_OriginStruct[i];
                    m_BulletStruct[i].m_Weapon[j].gameObject.SetActive(false);
                }
                if (m_BulletStruct[i].m_Type == type.ToString())
                {
                    typeCount = i;
                }
            }
            upgradeCount = new int[m_UpgradeStruct[typeCount].m_DetailUpgrade.Length];
            for (int i = 0; i < m_UpgradeStruct[typeCount].m_DetailUpgrade.Length; i++)
            {
                upgradeCount[i] = m_UpgradeStruct[typeCount].m_DetailUpgrade[i].m_DelayCount;
            }
            for (int i = 0; i < m_BulletStruct[typeCount].m_Weapon.Length; i++)
            {
                m_BulletStruct[typeCount].m_Weapon[i].gameObject.SetActive(true);
                m_Rotatement[i] = m_BulletStruct[typeCount].m_Weapon[i].GetComponent<Rotatement>();
                weaponPos[i] = m_Rotatement[i].transform.localPosition;
            }
        }
        m_WeaponPos.SetWeapon(m_WeaponLevel, typeint);
    }

    private void Update()
    {
        for (int i = 0; i < m_BulletStruct[typeCount].m_Weapon.Length; i++)
        {
            m_Rotatement[i].transform.localPosition = weaponPos[i] + m_Rotatement[i].transform.right * Mathf.Sin(sin[i]) * Mathf.Abs(absolute[i]) / 3;
            sin[i] -= 5 * Time.deltaTime;
            absolute[i] = Mathf.Lerp(absolute[i], 0, Time.deltaTime * 10);
        }

        if (Input.GetMouseButton(0) && attackDelay < 0)
        {
            StartCoroutine(m_BulletStruct[typeCount].m_Weapon.Length == 1 ? Shot(0) : MultiGun());
        }
        attackDelay -= Time.deltaTime;
    }

    private IEnumerator MultiGun()
    {
        for (int i = 0; i < m_BulletStruct[typeCount].m_Weapon.Length; i++)
        {
            StartCoroutine(Shot(i));
            yield return YieldInstructionCache.WaitForSeconds(m_BulletStruct[typeCount].m_MultiBurstCount);
        }
    }

    private IEnumerator Shot(int k)
    {
        m_AudioSource.clip = m_BulletStruct[typeCount].m_AudioClip;
        m_AudioSource.volume = m_BulletStruct[typeCount].m_Volume;
        m_AudioSource.Play();
        attackDelay = m_BulletStruct[typeCount].m_AttackDelay;
        for (int i = 0; i < m_BulletStruct[typeCount].m_BurstCount; i++)
        {
            sin[k] = -1;
            absolute[k] += 1;
            for (int j = 0; j < m_BulletStruct[typeCount].m_MultiCount; j++)
            {
                Get(m_BulletStruct[typeCount].m_Weapon[k], Quaternion.Euler(0, 0, m_Rotatement[k].m_Angle + m_BulletStruct[typeCount].m_SpreadAngle * (j - (m_BulletStruct[typeCount].m_MultiCount - 1) / 2)));
            }
            yield return YieldInstructionCache.WaitForSeconds(m_BulletStruct[typeCount].m_BurstSpeed);
        }
        yield return null;
    }

    public void Get(Transform pos, Quaternion rot)
    {
        GameObject select = null;

        foreach (GameObject item in pools)
        {
            if (!item.activeSelf)
            {
                select = item;
                select.SetActive(true);
                break;
            }
        }

        if (!select)
        {
            select = Instantiate(m_Projectile);
            pools.Add(select);
        }

        var projectile = select.GetComponent<Projectile>();

        projectile.line = projectile.transform.GetChild(typeCount);
        projectile.realDamage = m_BulletStruct[typeCount].m_Damage * m_Level.m_PersentDamage;
        projectile.penetrateCount = m_BulletStruct[typeCount].m_PenetrateCount;
        projectile.speed = m_BulletStruct[typeCount].m_ProjectileSpeed;
        projectile.rotateSpeed = m_BulletStruct[typeCount].m_ProjectileRotate;
        projectile.isPlayer = true;
        projectile.isTrail = typeCount == 1;

        for (int i = 0; i < m_BulletStruct.Length; i++)
        {
            select.transform.GetChild(i).gameObject.SetActive(i == typeCount);
        }

        select.transform.SetParent(m_ProjectileBundle);
        select.transform.rotation = rot;
        select.transform.position = pos.position + select.transform.right * m_BulletStruct[typeCount].m_RightPos;
    }
}
