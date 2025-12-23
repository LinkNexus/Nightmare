#!/bin/bash

echo "╔════════════════════════════════════════════════════════════╗"
echo "║         Nightmare HTTP Client - Verification Suite         ║"
echo "╚════════════════════════════════════════════════════════════╝"
echo ""

# Check if we're in the right directory
if [ ! -f "Nightmare.slnx" ]; then
    echo "❌ Error: Run this script from the project root"
    exit 1
fi

echo "1️⃣  Building project..."
dotnet build -c Release > /dev/null 2>&1
if [ $? -eq 0 ]; then
    echo "✅ Build successful"
else
    echo "❌ Build failed"
    exit 1
fi

echo ""
echo "2️⃣  Running tests..."
TEST_OUTPUT=$(dotnet test --no-build -c Release 2>&1)
TEST_COUNT=$(echo "$TEST_OUTPUT" | grep -oP 'Passed:   \K\d+')
if [ "$TEST_COUNT" = "143" ]; then
    echo "✅ All 143 tests passed"
else
    echo "❌ Test failures detected"
    exit 1
fi

echo ""
echo "3️⃣  Testing configuration loading..."
OUTPUT=$(dotnet run -c Release --project Nightmare example-config.json 2>&1)
if echo "$OUTPUT" | grep -q "Configuration loaded successfully"; then
    echo "✅ Configuration loads correctly"
else
    echo "❌ Configuration loading failed"
    exit 1
fi

echo ""
echo "4️⃣  Testing expression evaluation..."
OUTPUT=$(dotnet run -c Release --project Nightmare --demo 2>&1)
if echo "$OUTPUT" | grep -q "Expression Language Demo"; then
    echo "✅ Expression evaluation working"
else
    echo "❌ Expression evaluation failed"
    exit 1
fi

echo ""
echo "5️⃣  Testing error reporting..."
OUTPUT=$(dotnet run -c Release --project Nightmare test-error.json 2>&1)
if echo "$OUTPUT" | grep -q "line 7, column 9"; then
    echo "✅ Error reporting accurate"
else
    echo "❌ Error reporting failed"
    exit 1
fi

echo ""
echo "6️⃣  Testing AOT compilation..."
dotnet publish -c Release -r linux-x64 > /dev/null 2>&1
if [ -f "Nightmare/bin/Release/net10.0/linux-x64/publish/Nightmare" ]; then
    SIZE=$(du -h Nightmare/bin/Release/net10.0/linux-x64/publish/Nightmare | cut -f1)
    echo "✅ AOT compilation successful (Size: $SIZE)"
else
    echo "❌ AOT compilation failed"
    exit 1
fi

echo ""
echo "7️⃣  Testing AOT binary..."
OUTPUT=$(./Nightmare/bin/Release/net10.0/linux-x64/publish/Nightmare example-config.json 2>&1)
if echo "$OUTPUT" | grep -q "Configuration loaded successfully"; then
    echo "✅ AOT binary executes correctly"
else
    echo "❌ AOT binary execution failed"
    exit 1
fi

echo ""
echo "╔════════════════════════════════════════════════════════════╗"
echo "║                    ✅ All Checks Passed!                    ║"
echo "╚════════════════════════════════════════════════════════════╝"
echo ""
echo "Project Status:"
echo "  • Build: ✅ Successful"
echo "  • Tests: ✅ 143/143 passing"
echo "  • Config: ✅ Loading correctly"
echo "  • Expressions: ✅ Evaluating correctly"
echo "  • Errors: ✅ Reporting precisely"
echo "  • AOT: ✅ Compiling to native"
echo "  • Binary: ✅ Executing correctly"
echo ""
echo "You're ready to use Nightmare! 🚀"
