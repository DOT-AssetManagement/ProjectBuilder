using System.ComponentModel.DataAnnotations;

namespace ProjectBuilder.ViewModels
{
    public class ComparsionValidation :  ValidationAttribute
    {
        double _firstValue;
        private double _secondValue;
        string _propertyName = null;
        bool isItNumber = false;
        ComparesionType _comparesionType;
        public ComparsionValidation(double firstValue,ComparesionType comparesionType = ComparesionType.GreaterThen)
        {
            isItNumber = true;
             _firstValue = firstValue;
            _comparesionType = comparesionType;
        }
        public ComparsionValidation(double lessValue,double greaterValue, ComparesionType comparesionType = ComparesionType.GreaterAndLess)
        {
            isItNumber = true;
            _firstValue = greaterValue;
            _secondValue = lessValue;
            _comparesionType = comparesionType;
        }

        public ComparsionValidation(string propertyName, ComparesionType comparesionType = ComparesionType.GreaterThen)
        {
            isItNumber = false;
            _propertyName = propertyName;
            _comparesionType = comparesionType;
        }
        public ComparsionValidation(double firstValue,string propertyName, ComparesionType comparesionType = ComparesionType.GreaterAndLess)
        {
            isItNumber = false;
            _propertyName = propertyName;
            _firstValue=firstValue;
            _comparesionType = comparesionType;
        }
        private bool GearterThen(double value, bool equal = false) 
        {
            if(equal)
                return _firstValue <= value;
            return _firstValue < value;
        }
        private bool LessThen(double value, bool equal = false)
        {
            if (equal)
                return _firstValue >= value;
            return _firstValue > value;
        }
        private bool LessAndGreater(double value,bool equal = false)
        {
            if (equal)
                return _firstValue <= value && value <= _secondValue;
            return _firstValue < value && value < _secondValue;
        }
        private ValidationResult CompareValue(double result)
        {
            var success = false;
            switch (_comparesionType)
            {
                case ComparesionType.GreaterThen:
                    success = GearterThen(result);
                    break;
                case ComparesionType.LessThen:
                    success = LessThen(result);
                    break;
                case ComparesionType.GreateOrEqual:
                    success = GearterThen(result,true);
                    break;
                case ComparesionType.LessOrEqaul:
                    success = LessThen(result,true);
                    break;
                    case ComparesionType.GreaterAndLess:
                    success = LessAndGreater(result);
                    ErrorMessage = $"{_firstValue} <= P <= {_secondValue}";
                    break;
                case ComparesionType.GreaterOrEqualLessOrEqual:
                    success = LessAndGreater(result,true);
                    ErrorMessage = $"{_firstValue} <= P <= {_secondValue}";
                    break;
            }
            if (success)
                return ValidationResult.Success;
            return new(ErrorMessage);
        }
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if(!double.TryParse(value?.ToString(),out double result))
                return new(Constants.NUMBEREXPECTED);
            if (!isItNumber && (validationContext.DisplayName != _propertyName))
            {
               var propertyValue = validationContext.ObjectInstance.GetType().GetProperty(_propertyName).GetValue(validationContext.ObjectInstance);
                bool success = false;   
                if (_comparesionType != ComparesionType.GreaterOrEqualLessOrEqual && _comparesionType != ComparesionType.GreaterAndLess) 
                {
                   success = double.TryParse(propertyValue?.ToString(), out _firstValue);
                }
                else success = double.TryParse(propertyValue?.ToString(), out _secondValue);
                if (!success)
                    return ValidationResult.Success;              
            }
            return CompareValue(result);
        }
    }
}
