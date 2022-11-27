using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

[SerializeField]
public class Boosters : MonoBehaviour
{
    public enum BoosterType
    {
        Mix,
        Erase,
        Add,
        Count
    }

    [System.Serializable]
    public class Booster
    {
        public static readonly int max = 99;
        [HideInInspector]
        public Button btn;
        [HideInInspector]
        public Text label;
        [HideInInspector]
        public Animator animation;
        public BoosterType type;
        public int amount;
        public int price;
        public float cooldown = 10;

        public delegate void BoosterCallback(Booster booster);
        private StringBuilder stringBuilder = new StringBuilder(10);

        private float timer = 0;
        private BoosterCallback activate;
        private BoosterCallback release;

        private string Name()
        {
            string s = stringBuilder.AppendFormat("Booster.{0}", type).ToString();
            stringBuilder.Clear();
            return s;
        }

        public void Save()
        {
            Config.SaveInt(Name(), amount);
        }

        public void Load()
        {
            Config.LoadInt(Name(), out amount, amount);
            UpdateLable();
        }

        public void Fill(Transform child, BoosterCallback activate_call, BoosterCallback release_call)
        {
            btn = child.GetComponent<Button>();
            label = btn.transform.GetChild(1).GetComponent<Text>();
            animation = child.GetComponent<Animator>();
            activate = activate_call;
            release = release_call;
            btn.onClick.AddListener(delegate (){
                if (amount > 0)
                {
                    timer = cooldown;
                    btn.interactable = false;
                    activate(this);
                }
            });
            timer = 0.1f;
        }

        public void Update()
        {
            if(timer > 0)
            {
                timer -= Time.deltaTime;
                if(timer <= 0)
                {
                    btn.interactable = amount > 0;
                    release(this);
                }
            }
        }

        private void UpdateLable()
        {
            if (label != null)
            {
                label.text = amount.ToString();
            }
        }

        private void Inc()
        {
            if (amount < max)
            {
                amount++;
                animation.SetTrigger("Add");
                UpdateLable();
            }
            Save();
        }

        private void Dec()
        {
            if (amount > 0)
            {
                amount--;
                UpdateLable();
            }
            Save();
        }

        private void Mul(int val)
        {
            if (amount > 0)
            {
                amount = amount * val;
            }
            else
            {
                amount = 1;
            }
            UpdateLable();
            animation.SetTrigger("Add");
            Save();
        }
        public static Booster operator ++(Booster b)
        {
            b.Inc();
            return b;
        }

        public static Booster operator --(Booster b)
        {
            b.Dec();
            return b;
        }

        public static Booster operator *(Booster b, int val)
        {
            b.Mul(val);
            return b;
        }

    }

    public Booster[] boosters;
    Dictionary<BoosterType, int> lookupTable = new Dictionary<BoosterType, int>();

    public void InitBoosters(Booster.BoosterCallback activate, Booster.BoosterCallback release)
    {
        int size = Mathf.Min(transform.childCount, boosters.Length);
        for (int i = 0; i < size; i++)
        {
            boosters[i].Fill(transform.GetChild(i), activate, release);
            boosters[i].Load();
            lookupTable[boosters[i].type] = i;
        }
    }

    public void FillPrice(Text[] prices)
    {
        int size = Mathf.Min(prices.Length, boosters.Length);
        for (int i = 0; i < size; i++)
        {
            prices[i].text = boosters[i].price.ToString();
        }
    }

    public void FillAmount(Text[] counts)
    {
        int size = Mathf.Min(counts.Length, boosters.Length);
        for (int i = 0; i < size; i++)
        {
            counts[i].text = boosters[i].amount.ToString();
        }
    }

    public int Price(Boosters.BoosterType type)
    {
        return boosters[lookupTable[type]].price;
    }

    public int Index(Boosters.BoosterType type)
    {
        return lookupTable[type];
    }

    public int AddBooster(Boosters.BoosterType type)
    {
        return (boosters[Index(type)]++).amount;
    }
    public void AddBonus(int val)
    {
        if (val < 5)
        {
            return;
        }
        else if (val < 9)
        {
            for (int i = 6; i <= val; i++) // 3 - 8
            {
                boosters[Random.Range(0, boosters.Length)]++;
            }
        }
        else
        {
            for (int i = 0; i < boosters.Length; i++) { _ = boosters[i] * (val - 8); }
        }
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < boosters.Length; i++)
        {
            boosters[i].Update();
        }
    }
}
