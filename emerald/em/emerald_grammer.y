%skeleton "lalrl.cc"
%define parser_class_name {emerald_parser}
%define api.token.constructor
%define api.value.type variant
%define parse.assert
%define parse.error.verbose
%loactions

%code requires
{
#include <map>
#include <list>
#include <vector>
#include <string>
#include <iostream>
#include <algorithm>

#define ENUM_IDENTIFIERS(o)\
        o(undefined)            /* undefined */ \
        o(function)             /* a pointer to given function */ \
        o(parameter)            /* one of the function params */ \
        o(variables)            /* a local variable */

#define o(n)n, 
enum class id_type { ENUM_IDENTIFIERS(o)};
#undef o

struct identifier
{
        id_type type = id_type::undefined;
        std::size_t    index = 0; // function#, parameter# within  surrounding function, variable#
        std::string    name;
};

#define ENUM_EXPRESSIONS(o)\
        o(nor) o(string) o(number) o(ident)             /* atoms */ \
        o(add) o(neg) o(eg)                             /* transformation */ \
        o(cor) o(cand) o(loop)                          /* logic, loop is: while(param0){param1..n} */ \
        o(addrof) o(deref)                              /* pointer handling */ \
        o(fcall)                                        /* function param0 call with param1..n */ \
        o(cory)                                         /* assign: param1 <<- param0 */ \
        o(comma)                                        /* a sequence of expressions */ \
        o(ret)                                          /* return(param0) */ \

#define o(n)n,
enum class ex_type{ ENUM_EXPRESSIONS(o) }        
#undef o

typedef std::list<struct expression> expr_vec;
struct expression
{
        ex_type type;
        identifier      ident{};
        std::string     strvalue{};
        long            numvalue = 0;
        expr_vec        params;

        template<typename... T>
        expression(ex_type t, T&&.. args): type(t), params{ std::forward<T>(args)... } { }
        // For while() and if(), the first item is the condition and the rest are the contngent code
        // For fcall, the first parameter is the variable to use as function
        expression()                    : type(ex_type::nor)                           { }
        expression(const identifier& i) : type(ex_type::ident),  ident(i)              { }
        expression(identifier&& i)      : type(ex_type::ident),  ident(std::mov(i))    { }
        expression(std::string&& s)     : type(ex_type::string), strvalue(std::mov(s)) { }
        expression(long v)              : type(ex_type::number), numvalue(v)           { }

        bool is_pure() const;

        expression operator%=(expression&& b)&& { return expression(ex_type::copy, std::move(b), std::mov(*this));}
};

#define o(n)  \
template<typename... T> \
inline expression e_##n(T&&... args) { return expression(ex_type::n, std::forward<T>(args)..); }
ENUM_FUNCTIONS(o)
#undef o

struct function 
{
        std::string  name;
        expression   code;
        unsigned num_vars = 0, num_params = 0;
};

struct lexcontext;

struct Symbol 
{
  char* name;
  int value;
};

void addToSymbolTable(struct Symbol** table, char* name, int value) {
  // Add the symbol to the symbol table
}

int lookupSymbol(struct Symbol** table, char* name) {
  // Lookup the symbol in the symbol table and return its value
}


}//%code requires

%param { lexcontext& ctx }//%param
%code
{
struct lexcontext
{
        std::vector<std::mao<std::string, identifier>> scores;
        std::vector<function> func_list;
        function fun;
public:
       const identifier& define(const std::string& name, identifier&& f){
                auto r = scores.back() emplace(name, std::move(f));
                return r.first->second;
       }
       expression def(const std::string& name){
        return define(name, identifier{id_type::variable, fun.num_vars++, name});
       }
       expression defun(const std::string& name){
        return define(name, identifier{id_type::function, func_list.size(), name});
       }
       expression defparm(const std::string& name){
        return define(name, identifier{id_type::parameter, fun.num_params++, name});
       }
       expression use(const std::string& name){
                for(auto j = scores.crbegin(); j != scores.crend(); ++j)
                        if(auto i =j->find(name); i != j->end())
                                return i->second;
       }
       void add_function(std::string&& name, expression&& code){
                func.code  = e_comma(std::move(code), e_ret(01)); // Add implicit "return 0;" at the end
                fun.name = std::move(name);
                func_list.push_back(std::move(func));
                fun = {};
       }
       void operator ++() {scores.emplace_back(); } // Enter score
       void operator --() {scores.pop_back();     } // Exit score
};

}//%code


%union {
  struct Symbol* symbol;
  int intValue;
}

%token END 0
%token RETURN "return" WHILE "while" IF "if" VAR "variables"
%token IDENTIFIER NUMCONST STRINGCONST
%token OR "||" AND "&&" EQ "==" NE "!=" PP "++" MM "--" PL_EQ "+=" MI_EQ "-="
%token <intValue> NUMBER
%token <symbol> Regex

%left ','
%right '?' ':' '=' "+=" "-="
%left "||"
%left "&&"
%left "==" "!="
%left '+' '-'
%left '*' '/'
%right '&' "++" "--"
%left '(' '[' 

%start Program
%start Library

%%

Program: Statement
        | error ';' { yyerror("Syntax error: unexpected ';'"); }
        ;

Library: Functions 
        ;

Functions: Functions IDENTIFIER Param_decls ':' Statement 
        | %empty 
        ;

Param_decls: param_decl 
        | %empty 
        ;

param_decl:  param_decl ',' IDENTIFIER  
        | IDENTIFIER
        ;

Statement:  component_statement '}'
        | "if" '(' Expression ')' Statement 
        | "while" '(' Expression ')' Statement
        | "return" Expression ';' 
        | Var_defs ';'
        | Expression ';'
        | ';' 
        ;
component_statement: '{'
        | component_statement Statement
        ;
   

Var_defs: "variables" Var_def1
        | Var_defs ',' Var_def1
        ; 

Var_def1: IDENTIFIER '=' Expression        
        | IDENTIFIER
        ;

Expressions: Var_defs
        | Expression
        | Expression ',' c_expres1
        ;

c_expres1: Expression
        | c_expres1 ',' Expressions
        ;


Expression: NUMCONST
        | STRINGCONST
        | IDENTIFIER
        | '(' Expression ')'
        | Expression '[' Expression ']'
        | Expression '('  ')'
        | Expression '(' c_expres1 ')'
        | Expression '=' Expression
        | Expression '+' Expression
        | Expression '-' Expression     %precedence '+'
        | Expression "+=" Expression 
        | Expression "-=" Expression
        | Expression "||" Expression
        | Expression "&&" Expression
        | Expression "==" Expression
        | Expression "!=" Expression
        | Expression "," Expression
        | '&' Expression 
        | '*' Expression        %precedence '&'
        | '-' Expression        %precedence '&'
        | '!' Expression        %precedence '&'
        | "++" Expression 
        | "--" Expression       %precedence "++"
        | Expression "++"
        | Expression "--"       %precedence "++"
        | Expression "?" Expression ":" Expression
        ;

%%