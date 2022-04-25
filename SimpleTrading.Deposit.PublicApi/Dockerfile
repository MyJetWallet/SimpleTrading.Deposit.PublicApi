FROM mcr.microsoft.com/dotnet/aspnet:6.0-bullseye-slim
WORKDIR /app
COPY . .
ARG app_compilation_date
ARG app_version
ENV APP_COMPILATION_DATE=${app_compilation_date}
ENV APP_VERSION=${app_version}
ENTRYPOINT ["tail -f"]
#ENTRYPOINT ["dotnet", "SimpleTrading.Deposit.PublicApi.dll"]
# ENTRYPOINT ["dotnet", "/app/SimpleTrading.Deposit.PublicApi/bin/Release/net6.0/SimpleTrading.Deposit.PublicApi.dll"]
