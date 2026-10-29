using System;
using System.Collections.Generic;
using System.Text;

namespace ProjectBuilder.Helpers.Logger
{
    public interface IAppLogger<T>
    {
        void Info(string message, params object[] args);
        void Warning(string message, params object[] args);
        void Debug(string message, params object[] args);
        void Error(string message, params object[] args);
        void Error(Exception ex, string message, params object[] args);
        void Critical(Exception ex, string message, params object[] args);

        void DatabaseInfo(string message, params object[] args);
        void DatabaseWarning(string message, params object[] args);
        void DatabaseError(Exception ex, string message, params object[] args);

        void AuthInfo(string message, params object[] args);
        void AuthWarning(string message, params object[] args);
        void AuthError(Exception ex, string message, params object[] args);
    }
}
