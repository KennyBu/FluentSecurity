﻿using System;
using System.Linq.Expressions;
using FluentSecurity.Policy;

namespace FluentSecurity.TestHelper.Expectations
{
	public class HasTypeExpectation<TSecurityPolicy> : HasTypeExpectation where TSecurityPolicy : class, ISecurityPolicy
	{
		public Expression<Func<TSecurityPolicy, bool>> PredicateExpression { get; private set; }
		public Func<TSecurityPolicy, bool> Predicate { get; private set; }
		
		public HasTypeExpectation() : base(typeof(TSecurityPolicy), false)
		{
			PredicateExpression = securityPolicy => securityPolicy.GetType() == Type;
			Predicate = PredicateExpression.Compile();
		}

		public HasTypeExpectation(Expression<Func<TSecurityPolicy, bool>> predicateExpression) : base(typeof(TSecurityPolicy), true)
		{
			PredicateExpression = predicateExpression;
			Predicate = PredicateExpression.Compile();
		}

		protected override bool EvaluatePredicate(ISecurityPolicy securityPolicy)
		{
			var policyToEvaluate = GetPolicyToEvaluate(securityPolicy);
			var policy = policyToEvaluate as TSecurityPolicy;
			return policy != null && Predicate.Invoke(policy);
		}

		private static ISecurityPolicy GetPolicyToEvaluate(ISecurityPolicy securityPolicy)
		{
			return securityPolicy is ILazySecurityPolicy
			       	? ((ILazySecurityPolicy)securityPolicy).Load()
			       	: securityPolicy;
		}

		public override string GetPredicateDescription()
		{
			return PredicateExpression.ToString();
		}
	}

	public abstract class HasTypeExpectation : TypeExpectation
	{
		protected HasTypeExpectation(Type type, bool isPredicateExpectation) : base(type, isPredicateExpectation) {}
	}
}