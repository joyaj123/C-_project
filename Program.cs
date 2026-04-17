var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// SESSION
builder.Services.AddSession();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();      // Combined: UseStaticAssets + UseStaticFiles
app.UseRouting();

// SESSION
app.UseSession();

app.UseAuthorization();

// Default route - using LogIn as default controller (can be changed)
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=LogIn}/{action=Login}/{id?}");

app.Run();