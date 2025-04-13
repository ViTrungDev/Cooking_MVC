# Sử dụng image của .NET làm base image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

# Sử dụng image SDK để build ứng dụng
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy file .csproj vào thư mục /src
COPY ./Cooking.csproj ./  
# Restore dependencies
RUN dotnet restore "Cooking.csproj"

# Copy toàn bộ mã nguồn vào container
COPY . .

# Build dự án với cấu hình Release
RUN dotnet build "Cooking.csproj" -c Release -o /app/build

# Publish ứng dụng
FROM build AS publish
RUN dotnet publish "Cooking.csproj" -c Release -o /app/publish

# Tạo final image
FROM base AS final
WORKDIR /app

# Copy file đã publish vào image final
COPY --from=publish /app/publish .

# Cài đặt entrypoint cho ứng dụng
ENTRYPOINT ["dotnet", "Cooking.dll"]
