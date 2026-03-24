using BattleSimulator.Core.Models;

namespace BattleSimulator.Core.Models.Characters
{
    /// <summary>
    /// 法师类，远程魔法伤害角色
    /// </summary>
    public class Mage : Entity
    {
        public Mage() : base()
        {
            // 设置法师基础属性
            Name = "法师";
            MaxHealth = 100;
            Health = MaxHealth;
            Attack = 40;
            Defense = 10;
            Speed = 20;
            HitRate = 85;
            DodgeRate = 15;
            CritRate = 20;
            CritDamage = 75;
            
            // 设置法师所属阵营
            Faction = Faction.Ally;
            
            // 初始化法师技能
            InitializeSkills();
        }
        
        /// <summary>
        /// 初始化法师技能
        /// </summary>
        private void InitializeSkills()
        {
            // 火球术技能
            var fireballSkill = new FireballSkill();
            Skills.Add(fireballSkill);
            
            // 冰霜新星技能
            var frostNovaSkill = new FrostNovaSkill();
            Skills.Add(frostNovaSkill);
            
            // 魔法护盾技能
            var magicShieldSkill = new MagicShieldSkill();
            Skills.Add(magicShieldSkill);
        }
        
        /// <summary>
        /// 施法者特性：每次使用技能后有几率获得魔法强化
        /// </summary>
        public void OnSkillUsed()
        {
            // 20%的几率获得魔法强化效果
            if (IsAlive && new Random().Next(100) < 20)
            {
                var magicEmpowerment = new MagicEmpowermentEffect
                {
                    Duration = 2,
                    SpellPowerIncrease = 15
                };
                
                // 应用状态效果
                var existingEffect = StatusEffects.FirstOrDefault(e => e.Id == magicEmpowerment.Id);
                if (existingEffect != null)
                {
                    existingEffect.CurrentDuration = Math.Max(existingEffect.CurrentDuration, magicEmpowerment.Duration);
                }
                else
                {
                    magicEmpowerment.Initialize();
                    magicEmpowerment.Apply(this);
                    StatusEffects.Add(magicEmpowerment);
                }
            }
        }
    }
    
    /// <summary>
    /// 火球术技能 - 法师的基础攻击技能
    /// </summary>
    public class FireballSkill : Skill
    {
        public FireballSkill()
        {
            Id = "fireball";
            Name = "火球术";
            Description = "发射一个火球攻击单个敌人，并造成燃烧效果";
            ManaCost = 15;
            Cooldown = 2;
            TargetType = SkillTargetType.SingleEnemy;
            EffectType = SkillEffectType.Damage;
        }
        
        protected override int CalculateBaseDamage(Entity caster, Entity target)
        {
            // 火球术造成魔法伤害
            return (int)(caster.Attack * 1.2 - target.Defense / 4);
        }
        
        protected override void ApplyDamageEffect(Entity caster, List<Entity> targets, SkillResult result)
        {
            base.ApplyDamageEffect(caster, targets, result);
            
            // 为目标施加燃烧效果
            var burnEffect = new BurnEffect
            {
                Duration = 2,
                DamagePerTurn = 8
            };
            
            foreach (var target in targets)
            {
                if (target.IsAlive && new Random().Next(100) < 70) // 70%的几率施加燃烧效果
                {
                    result.StatusEffectsApplied.Add(burnEffect.Clone());
                    target.BattleLog.Add($"{target.Name} 被火球术点燃了！");
                }
            }
            
            // 触发法师特性
            if (caster is Mage mage)
            {
                mage.OnSkillUsed();
            }
        }
    }
    
    /// <summary>
    /// 冰霜新星技能 - 法师的控制技能
    /// </summary>
    public class FrostNovaSkill : Skill
    {
        public FrostNovaSkill()
        {
            Id = "frost_nova";
            Name = "冰霜新星";
            Description = "释放冰霜能量，对周围敌人造成伤害并减速";
            ManaCost = 20;
            Cooldown = 4;
            TargetType = SkillTargetType.AllEnemies;
            EffectType = SkillEffectType.Damage;
        }
        
        protected override int CalculateBaseDamage(Entity caster, Entity target)
        {
            // 冰霜新星造成较低的伤害，但有控制效果
            return (int)(caster.Attack * 0.6 - target.Defense / 3);
        }
        
        protected override void ApplyDamageEffect(Entity caster, List<Entity> targets, SkillResult result)
        {
            base.ApplyDamageEffect(caster, targets, result);
            
            // 为目标施加减速效果
            var slowEffect = new SlowEffect
            {
                Duration = 3,
                SpeedReduction = 5
            };
            
            foreach (var target in targets)
            {
                if (target.IsAlive)
                {
                    result.StatusEffectsApplied.Add(slowEffect.Clone());
                    target.BattleLog.Add($"{target.Name} 被冰霜新星减速了！");
                }
            }
            
            // 触发法师特性
            if (caster is Mage mage)
            {
                mage.OnSkillUsed();
            }
        }
    }
    
    /// <summary>
    /// 魔法护盾技能 - 法师的防御技能
    /// </summary>
    public class MagicShieldSkill : Skill
    {
        public MagicShieldSkill()
        {
            Id = "magic_shield";
            Name = "魔法护盾";
            Description = "创造一个魔法护盾，吸收伤害并提升魔法抗性";
            ManaCost = 10;
            Cooldown = 5;
            TargetType = SkillTargetType.Self;
            EffectType = SkillEffectType.Buff;
        }
        
        protected override SkillResult ApplyEffect(Entity caster, List<Entity> targets)
        {
            var result = new SkillResult { Success = true };
            
            // 为自己施加魔法护盾效果
            var shieldEffect = new MagicShieldEffect
            {
                Duration = 3,
                DamageAbsorption = 25
            };
            
            shieldEffect.Initialize();
            shieldEffect.Apply(caster);
            caster.StatusEffects.Add(shieldEffect);
            
            result.StatusEffectsApplied.Add(shieldEffect);
            result.Message = $"{caster.Name} 开启了魔法护盾！\n";
            
            // 触发法师特性
            if (caster is Mage mage)
            {
                mage.OnSkillUsed();
            }
            
            return result;
        }
    }
    
    /// <summary>
    /// 燃烧效果
    /// </summary>
    public class BurnEffect : StatusEffect
    {
        public int DamagePerTurn { get; set; } = 8;
        
        public BurnEffect()
        {
            Id = "burn";
            Name = "燃烧";
            Description = "每回合受到魔法伤害";
            Duration = 2;
            IsDebuff = true;
            IsStackable = true;
            MaxStacks = 3;
        }
        
        public override void Apply(Entity target)
        {
            // 首次应用时的效果
        }
        
        public override bool Update(Entity target)
        {
            // 每回合造成伤害
            int totalDamage = DamagePerTurn * CurrentStacks;
            target.TakeDamage(totalDamage);
            
            // 更新状态信息
            target.BattleLog.Add($"{target.Name} 受到了 {totalDamage} 点燃烧伤害！");
            
            return base.Update(target);
        }
        
        public override StatusEffect Clone()
        {
            return new BurnEffect
            {
                Id = this.Id,
                Name = this.Name,
                Description = this.Description,
                Duration = this.Duration,
                IsDebuff = this.IsDebuff,
                IsStackable = this.IsStackable,
                MaxStacks = this.MaxStacks,
                DamagePerTurn = this.DamagePerTurn
            };
        }
    }
    
    /// <summary>
    /// 减速效果
    /// </summary>
    public class SlowEffect : StatusEffect
    {
        public int SpeedReduction { get; set; } = 5;
        
        public SlowEffect()
        {
            Id = "slow";
            Name = "减速";
            Description = "速度降低";
            Duration = 3;
            IsDebuff = true;
            IsStackable = true;
            MaxStacks = 2;
        }
        
        public override void Apply(Entity target)
        {
            // 降低目标速度
            target.Speed -= SpeedReduction;
            target.BattleLog.Add($"{target.Name} 的速度降低了 {SpeedReduction} 点！");
        }
        
        public override void OnExpire(Entity target)
        {
            // 恢复目标速度
            target.Speed += SpeedReduction * CurrentStacks;
            target.BattleLog.Add($"{target.Name} 从减速效果中恢复了！");
        }
        
        public override StatusEffect Clone()
        {
            return new SlowEffect
            {
                Id = this.Id,
                Name = this.Name,
                Description = this.Description,
                Duration = this.Duration,
                IsDebuff = this.IsDebuff,
                IsStackable = this.IsStackable,
                MaxStacks = this.MaxStacks,
                SpeedReduction = this.SpeedReduction
            };
        }
    }
    
    /// <summary>
    /// 魔法护盾效果
    /// </summary>
    public class MagicShieldEffect : StatusEffect
    {
        public int DamageAbsorption { get; set; } = 25;
        
        public MagicShieldEffect()
        {
            Id = "magic_shield";
            Name = "魔法护盾";
            Description = "吸收伤害并提升魔法抗性";
            Duration = 3;
            IsDebuff = false;
            IsStackable = false;
        }
        
        public override void Apply(Entity target)
        {
            // 魔法护盾主要在TakeDamage方法中生效
            target.BattleLog.Add($"{target.Name} 获得了魔法护盾的保护！");
        }
        
        public override void OnExpire(Entity target)
        {
            target.BattleLog.Add($"{target.Name} 的魔法护盾消失了！");
        }
        
        public override StatusEffect Clone()
        {
            return new MagicShieldEffect
            {
                Id = this.Id,
                Name = this.Name,
                Description = this.Description,
                Duration = this.Duration,
                IsDebuff = this.IsDebuff,
                IsStackable = this.IsStackable,
                DamageAbsorption = this.DamageAbsorption
            };
        }
    }
    
    /// <summary>
    /// 魔法强化效果
    /// </summary>
    public class MagicEmpowermentEffect : StatusEffect
    {
        public int SpellPowerIncrease { get; set; } = 15;
        
        public MagicEmpowermentEffect()
        {
            Id = "magic_empowerment";
            Name = "魔法强化";
            Description = "法术伤害提升";
            Duration = 2;
            IsDebuff = false;
            IsStackable = true;
            MaxStacks = 2;
        }
        
        public override void Apply(Entity target)
        {
            // 增加目标攻击力（代表法术强度）
            target.Attack += SpellPowerIncrease;
            target.BattleLog.Add($"{target.Name} 的魔法强度提升了 {SpellPowerIncrease} 点！");
        }
        
        public override void OnExpire(Entity target)
        {
            // 恢复目标攻击力
            target.Attack -= SpellPowerIncrease * CurrentStacks;
            target.BattleLog.Add($"{target.Name} 的魔法强化效果消失了！");
        }
        
        public override StatusEffect Clone()
        {
            return new MagicEmpowermentEffect
            {
                Id = this.Id,
                Name = this.Name,
                Description = this.Description,
                Duration = this.Duration,
                IsDebuff = this.IsDebuff,
                IsStackable = this.IsStackable,
                MaxStacks = this.MaxStacks,
                SpellPowerIncrease = this.SpellPowerIncrease
            };
        }
    }
}