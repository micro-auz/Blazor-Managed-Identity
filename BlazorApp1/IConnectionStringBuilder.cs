﻿namespace BlazorApp1
{
    public interface IConnectionStringBuilder
    {
        Task<string> GetConnectionString(bool isProduction);

        Task<string> GetConnectionString2(bool isProduction);

    }
}
