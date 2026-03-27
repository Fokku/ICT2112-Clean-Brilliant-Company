using System;
using CleanBrilliant.Models;
using CleanBrilliant.Interfaces;

namespace CleanBrilliant.Services
{
    public class CO2ViewControl
    {
       
        private readonly ILayoutManager _layoutViewManager;
        private readonly ICoefficientManager _coefficientManager;

       
        public CO2ViewControl(ILayoutManager layoutViewManager, ICoefficientManager coefficientManager)
        {
            _layoutViewManager = layoutViewManager;
            _coefficientManager = coefficientManager;
        }

        
        public DashboardLayout fetchDashboardLayout() 
        { 
          
            return _layoutViewManager.getLayout(1); 
        }

       
        public void updateDateRange(DateOnly startDate, DateOnly endDate) 
        { 
            var currentCoefficient = _coefficientManager.getEmission("");
            
            
        }
    }
}