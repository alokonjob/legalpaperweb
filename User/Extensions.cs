using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;

namespace User
{
    public static class UserRoleClaimsExtensions
    {
        public static bool IsFinanceUser(this ClaimsPrincipal User)
        {
            return User.HasClaim("access", "finance");
        }

        public static bool IsConsultant(this ClaimsPrincipal User)
        {
            return User.IsInRole("Consultant");
        }

        public static bool IsCaseManager(this ClaimsPrincipal User)
        {
            return User.IsInRole("CaseManager") && User.HasClaim("access", "caselist") && User.HasClaim("access", "caseupdate");
        }

        public static bool IsWebAdmin(this ClaimsPrincipal User)
        {
            return User.IsInRole("Staff") && User.HasClaim("access", "webadmin");
        }
        public static bool IsFounder(this ClaimsPrincipal User)
        {
            return User.IsInRole("Founder") && User.HasClaim("access", "founder");
        }

    }

    public static class DateDefaults
    { 
        public static DateTime GetDefaultDate() => DateTime.Parse("01/01/2020");

        public static bool IsDefaultDate(this DateTime dateToCompare) => DateTime.Compare(dateToCompare, GetDefaultDate()) == 0;

        public static DateTime SetDayStartDate(this DateTime DateToChange) => DateTime.Parse(DateToChange.ToShortDateString().Trim() + " 00:00:00.000");
        public static DateTime SetDayEndDate(this DateTime DateToChange) => DateTime.Parse(DateToChange.ToShortDateString().Trim() + " 23:59:59.990");
    }
}
