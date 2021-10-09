FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR "/app/"
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR "/src/"
# --- dependencies ---
COPY ["Fibonacci.Common/Fibonacci.Common.csproj", "Fibonacci.Common/"]
RUN dotnet restore "Fibonacci.Common/Fibonacci.Common.csproj"
COPY ["Fibonacci.Common/", "Fibonacci.Common/"]

# --- target ---
COPY ["Fibonacci.Rest/Fibonacci.Rest.csproj", "Fibonacci.Rest/"]
RUN dotnet restore "Fibonacci.Rest/Fibonacci.Rest.csproj"

COPY ["Fibonacci.Rest/", "Fibonacci.Rest/"]
WORKDIR "/src/Fibonacci.Rest/"
#RUN dotnet build "Fibonacci.Rest.csproj" -c Release -o "/app/publish/"

FROM build AS publish
RUN dotnet publish "Fibonacci.Rest.csproj" -c Release -o "/app/publish/"

FROM base AS final
WORKDIR "/app/"
COPY --from=publish "/app/publish/" .
ENTRYPOINT ["dotnet", "Fibonacci.Rest.dll"]