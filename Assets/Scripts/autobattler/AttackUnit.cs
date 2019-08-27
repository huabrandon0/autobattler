using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System.Linq;

namespace TeamfightTactics
{
    public class AttackUnit : TileUnit
    {
        [SerializeField]
        NavMeshAgent _navMeshAgent;

        IAttackUnitState _currentState;
        IAttackUnitState _zombieState;
        IAttackUnitState _idleState;
        IAttackUnitState _aggroState;
        IAttackUnitState _attackingState;

        List<AttackUnit> _potentialTargets;

        AttackUnit _target;

        float _health;

        bool Alive
        {
            get
            {
                return _health > 0f;
            }
        }

        float _timeOfLastAttack = float.MinValue;
        
        float TimeBetweenAttacks
        {
            get
            {
                return 1f / AttackUnitData.baseAttackRate;
            }
        }

        public AttackUnitData AttackUnitData
        {
            get
            {
                return tileUnitData as AttackUnitData;
            }
        }

        protected override void Awake()
        {
            base.Awake();

            _idleState = new IdleState(this);
            _aggroState = new AggroState(this);
            _attackingState = new AttackingState(this);
            _zombieState = new ZombieState(this);
            _currentState = _zombieState;

            _navMeshAgent.updateRotation = false;
            _animator = GetComponentInChildren<Animator>();
        }

        void Start()
        {
            _health = AttackUnitData.baseHealth;
        }

        void OnEnable()
        {
            AttackUnitManager.Instance.RegisterAttackUnit(this);
        }

        void OnDisable()
        {
            AttackUnitManager.Instance.DeregisterAttackUnit(this);
        }

        void Update()
        {
            _currentState.Update();
        }
        
        void AnimateAttack()
        {
            if (_animator)
                _animator.Play("Attack", -1);
        }

        void AnimateWalk()
        {
            if (_animator)
                _animator.Play("Walk", -1);
        }

        void AnimateIdle()
        {
            if (_animator)
                _animator.Play("Idle", -1);
        }

        public void AnimateDeath()
        {
            if (_animator)
                _animator.Play("Death", -1);
        }

        public void Enable()
        {
            _navMeshAgent.enabled = true;
            _currentState.Enable();
        }

        public void Disable()
        {
            _navMeshAgent.enabled = false;
            _currentState.Disable();
        }

        public void Damage(float amount)
        {
            if (!Alive)
                return;

            _health -= amount;

            if (!Alive)
            {
                _characterAnimationEventCalls.OnAttack.RemoveListener(DamageTarget);
                _currentState.Disable();
                AnimateDeath();
                StartCoroutine(Die());
            }
        }

        void DamageTarget()
        {
            _target.Damage(AttackUnitData.baseAttack);
        }

        IEnumerator Die()
        {
            yield return new WaitForSeconds(0.5f);
            GameManager.Instance.DestroyUnit(this);
        }

        interface IAttackUnitState
        {
            void Update();
            void Enable();
            void Disable();
        }

        class ZombieState : IAttackUnitState
        {
            AttackUnit _attackUnit;

            public ZombieState(AttackUnit attackUnit)
            {
                _attackUnit = attackUnit;
            }

            public void Update()
            {

            }

            public void Enable()
            {
                _attackUnit._currentState = _attackUnit._idleState;
                _attackUnit.AnimateIdle();
            }

            public void Disable()
            {

            }
        }

        class IdleState : IAttackUnitState
        {
            AttackUnit _attackUnit;

            public IdleState(AttackUnit attackUnit)
            {
                _attackUnit = attackUnit;
            }

            public void Update()
            {
                AttackUnit closest = GameManager.Instance.ActiveAttackUnits
                    .Where(x => x != _attackUnit && x.Alive && x.Key != _attackUnit.Key)
                    .OrderBy(x => (_attackUnit.transform.position - x.transform.position).magnitude)
                    .FirstOrDefault();

                if (closest)
                {
                    _attackUnit._target = closest;
                    _attackUnit._navMeshAgent.enabled = true;
                    _attackUnit._navMeshAgent.SetDestination(_attackUnit._target.transform.position);
                    _attackUnit._currentState = _attackUnit._aggroState;
                    _attackUnit.AnimateWalk();
                    return;
                }
            }

            public void Enable()
            {

            }

            public void Disable()
            {
                _attackUnit._navMeshAgent.enabled = false;
                _attackUnit._currentState = _attackUnit._zombieState;
                _attackUnit.AnimateIdle();
            }
        }

        class AggroState : IAttackUnitState
        {
            AttackUnit _attackUnit;

            public AggroState(AttackUnit attackUnit)
            {
                _attackUnit = attackUnit;
            }

            public void Update()
            {
                AttackUnit target = _attackUnit._target;
                NavMeshAgent navMeshAgent = _attackUnit._navMeshAgent;

                if (!target.Alive || !target)
                {
                    _attackUnit._navMeshAgent.enabled = false;
                    _attackUnit._currentState = _attackUnit._idleState;
                    _attackUnit.AnimateIdle();
                    return;
                }
                else if ((_attackUnit.transform.position - target.transform.position).magnitude <= _attackUnit.AttackUnitData.baseAttackRange)
                {
                    _attackUnit._navMeshAgent.enabled = false;
                    _attackUnit._currentState = _attackUnit._attackingState;
                    _attackUnit.AnimateIdle();
                    return;
                }

                navMeshAgent.SetDestination(target.transform.position);
                
                if (_attackUnit._navMeshAgent.velocity.sqrMagnitude > Mathf.Epsilon)
                    _attackUnit.transform.rotation = Quaternion.Lerp(_attackUnit.transform.rotation, Quaternion.LookRotation(_attackUnit._navMeshAgent.velocity.normalized), 0.1f);
            }

            public void Enable()
            {

            }

            public void Disable()
            {
                _attackUnit._navMeshAgent.enabled = false;
                _attackUnit._currentState = _attackUnit._zombieState;
                _attackUnit.AnimateIdle();
            }
        }

        class AttackingState : IAttackUnitState
        {
            AttackUnit _attackUnit;

            public AttackingState(AttackUnit attackUnit)
            {
                _attackUnit = attackUnit;
            }

            public void Update()
            {
                AttackUnit target = _attackUnit._target;
                NavMeshAgent navMeshAgent = _attackUnit._navMeshAgent;

                if (!target.Alive || !target)
                {
                    _attackUnit._characterAnimationEventCalls.OnAttack.RemoveListener(_attackUnit.DamageTarget);
                    _attackUnit._navMeshAgent.enabled = false;
                    _attackUnit._currentState = _attackUnit._idleState;
                    _attackUnit.AnimateIdle();
                    return;
                }
                else if ((_attackUnit.transform.position - target.transform.position).magnitude > _attackUnit.AttackUnitData.baseAttackRange)
                {
                    _attackUnit._characterAnimationEventCalls.OnAttack.RemoveListener(_attackUnit.DamageTarget);
                    navMeshAgent.enabled = true;
                    navMeshAgent.SetDestination(target.transform.position);
                    _attackUnit._currentState = _attackUnit._aggroState;
                    _attackUnit.AnimateWalk();
                    return;
                }

                if (Time.time - _attackUnit._timeOfLastAttack > _attackUnit.TimeBetweenAttacks)
                {
                    _attackUnit.AnimateAttack();
                    _attackUnit._characterAnimationEventCalls.OnAttack.AddListener(_attackUnit.DamageTarget);
                    _attackUnit._timeOfLastAttack = Time.time;
                }

                _attackUnit.transform.rotation = Quaternion.LookRotation(target.transform.position - _attackUnit.transform.position);
            }

            public void Enable()
            {

            }

            public void Disable()
            {
                _attackUnit._navMeshAgent.enabled = false;
                _attackUnit._currentState = _attackUnit._zombieState;
                _attackUnit.AnimateIdle();
            }
        }
    }
}
