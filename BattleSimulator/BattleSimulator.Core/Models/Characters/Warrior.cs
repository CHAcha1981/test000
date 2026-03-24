using BattleSimulator.Core.Models;

namespace BattleSimulator.Core.Models.Characters
{
    /// <summary>
    /// 战士类，近战物理伤害角色
    /// </summary>
    public class Warrior : Entity
    {
        public Warrior() : base()
        {
            // 设置战士基础属性
            Name = "战士";
            MaxHealth = 150;
            Health = MaxHealth;
            Attack = 30;
            Defense = 20;
            Speed = 15;
            HitRate = 90;
            DodgeRate = 10;
            CritRate = 15;
            CritDamage = 50;
            
            // 设置战士所属阵营
            Faction = Faction.Ally;
            
            // 初始化战士技能
            InitializeSkills();
        }
        
        /// <summary>
        /// 初始化战士技能
        /// </summary>
        private void InitializeSkills()
        {
            // 旋风斩技能
            var whirlwindSkill = new WhirlwindSkill();
            Skills.Add(whirlwindSkill);
            
            // 嘲讽技能
            var tauntSkill = new TauntSkill();
            Skills.Add(tauntSkill);
            
            // 复仇技能
            var revengeSkill = new RevengeSkill();
            Skills.Add(revengeSkill);
        }
        
        /// <summary>
        /// 承受伤害（战士特性：受到伤害时有几率获得怒气）
        /// </summary>
        public override int TakeDamage(int damage)
        {
            int actualDamage = base.TakeDamage(damage);
            
            // 战士特性：受到伤害时有几率获得攻击力提升
            if (IsAlive && actualDamage > 0)
            {
                // 30%的几率获得攻击力提升
                if (new Random().Next(100) < 30)
                {
                    var rageEffect = new AttackBoostEffect
                    {
                        Duration = 2,
                        AttackIncrease = 5
                    };
                    
                    // 应用状态效果
                    var existingEffect = StatusEffects.FirstOrDefault(e => e.Id == rageEffect.Id);
                    if (existingEffect != null)
                    {
                        existingEffect.CurrentDuration = Math.Max(existingEffect.CurrentDuration, rageEffect.Duration);
                    }
                    else
                    {
                        rageEffect.Initialize();
                        rageEffect.Apply(this);
                        StatusEffects.Add(rageEffect);
                    }
                }
            }
            
            return actualDamage;
        }
    }
    
    /// <summary>
    /// 旋风斩技能 - 战士的范围攻击技能
    /// </summary>
    public class WhirlwindSkill : Skill
    {
        public WhirlwindSkill()
        {
            Id = "whirlwind";
            Name = "旋风斩";
            Description = "对周围所有敌人造成伤害";
            ManaCost = 10;
            Cooldown = 3;
            TargetType = SkillTargetType.AllEnemies;
            EffectType = SkillEffectType.Damage;
        }
        
        protected override int CalculateBaseDamage(Entity caster, Entity target)
        {
            // 旋风斩造成的伤害略低于普通攻击，但可以攻击多个目标
            return (int)(caster.Attack * 0.8 - target.Defense / 3);
        }
    }
    
    /// <summary>
    /// 嘲讽技能 - 战士的控制技能
    /// </summary>
    public class TauntSkill : Skill
    {
        public TauntSkill()
        {
            Id = "taunt";
            Name = "嘲讽";
            Description = "强制敌人攻击自己，并提升防御力";
            ManaCost = 5;
            Cooldown = 4;
            TargetType = SkillTargetType.AllEnemies;
            EffectType = SkillEffectType.Buff;
        }
        
        protected override SkillResult ApplyEffect(Entity caster, List<Entity> targets)
        {
            var result = new SkillResult { Success = true };
            
            // 提升自己的防御力
            var defenseBuff = new DefenseBoostEffect
            {
                Duration = 2,
                DefenseIncrease = 10
            };
            
            defenseBuff.Initialize();
            defenseBuff.Apply(caster);
            caster.StatusEffects.Add(defenseBuff);
            
            result.StatusEffectsApplied.Add(defenseBuff);
            
            // 强制敌人攻击自己
            foreach (var target in targets)
            {
                if (target.IsAlive)
                {
                    result.Message += $"{caster.Name} 嘲讽了 {target.Name}！\n";
                }
            }
            
            return result;
        }
    }
    
    /// <summary>
    /// 复仇技能 - 战士的反击技能
    /// </summary>
    public class RevengeSkill : Skill
    {
        public RevengeSkill()
        {
            Id = "revenge";
            Name = "复仇";
            Description = "根据已损失的生命值提升攻击力并攻击敌人";
            ManaCost = 15;
            Cooldown = 5;
            TargetType = SkillTargetType.SingleEnemy;
            EffectType = SkillEffectType.Damage;
        }
        
        protected override int CalculateBaseDamage(Entity caster, Entity target)
        {
            // 复仇技能的伤害根据已损失的生命值提升
            int missingHealth = caster.MaxHealth - caster.Health;
            float damageMultiplier = 1.0f + (missingHealth / (float)caster.MaxHealth);
            
            return (int)((caster.Attack - target.Defense / 2) * damageMultiplier);
        }
    }
    
    /// <summary>
    /// 防御力提升状态效果
    /// </summary>
    public class DefenseBoostEffect : StatusEffect
    {
        public int DefenseIncrease { get; set; } = 10;
        
        public DefenseBoostEffect()
        {
            Id = "defense_boost";
            Name = "防御力提升";
            Description = "防御力临时增加";
            Duration = 2;
            IsDebuff = false;
            IsStackable = false;
        }
        
        public override void Apply(Entity target)
        {
            // 增加目标防御力
            target.Defense += DefenseIncrease;
            target.BattleLog.Add($"{target.Name} 的防御力提升了 {DefenseIncrease} 点！");
        }
        
        public override void OnExpire(Entity target)
        {
            // 恢复目标防御力
            target.Defense -= DefenseIncrease;
            target.BattleLog.Add($"{target.Name} 的防御力提升效果消失了！");
        }
        
        public override StatusEffect Clone()
        {
            return new DefenseBoostEffect
            {
                Id = this.Id,
                Name = this.Name,
                Description = this.Description,
                Duration = this.Duration,
                IsDebuff = this.IsDebuff,
                IsStackable = this.IsStackable,
                DefenseIncrease = this.DefenseIncrease
            };
        }
    }
}