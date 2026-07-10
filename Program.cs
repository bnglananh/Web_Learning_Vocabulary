using Web_Learning_Vocabulary.Services;

var builder = WebApplication.CreateBuilder(args);

// ✅ Add custom services
builder.Services.AddScoped<IVocabularyService, VocabularyService>();
builder.Services.AddScoped<IQuizService, QuizService>();
builder.Services.AddScoped<IPassageExerciseService, PassageExerciseService>();

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Add CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Add vocabulary generator service
builder.Services.AddHttpClient<VocabularyGeneratorService>();
builder.Services.AddScoped<IVocabularyGeneratorService>(provider =>
    provider.GetRequiredService<VocabularyGeneratorService>());

// Add logging
builder.Services.AddLogging();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// Enable CORS
app.UseCors("AllowAll");

// Serve static files (wwwroot)
app.UseStaticFiles();

app.UseAuthorization();

app.MapControllers();

// Map default to index.html
app.MapFallbackToFile("index.html");

app.Run();
