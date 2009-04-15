import clr
clr.AddReference('PLR.dll')
from PLR import *
from PLR.AST import *
from PLR.AST.Actions import *
from PLR.AST.Processes import *
from PLR.Runtime import *
from PLR.Compilation import *
from System.Collections.Generic import *
from System.Reflection import *
from System.Reflection.Emit import *
from System import Array, Type, Object
import sys

#token types
LOCATION = 'LOCATION'
VARIABLE = 'VARIABLE'
BOUNDVARIABLE = 'BOUNDVARIABLE'
NUMBER = 'NUMBER'
DOUBLESEMI = '::'
DOUBLEPIPE = '||'
LANGLE = '<'
RANGLE = '>'
DOT = '.'
LPAREN = '('
RPAREN = ')'
COMMA = ','
PLUS = '+'
PIPE = '|'
BANG = '!'
MINUS = '-'
ASTERISK = '*'
DIVIDE = '/'
OUT = 'out'
IN = 'in'
READ = 'read'
AT = '@'
COMMENTSTART = '#'
COMMENTEND = '\n'
EOF = 'EOF'

class Token:
    def __init__(self, val, type, line, col):
        self.val = val
        self.type = type
        self.line = line
        self.col = col
        
    def __str__(self):
        return '%s (%d,%d)' % (self.val, self.line, self.col)
        
class Lexer:
    
    def __init__(self, buffer):
        self.buffer = buffer
        self.pos = 0
        self.line = 1
        self.col = 1
    
    def update_pos(self, ch):
        self.pos += 1
        self.col += 1
        if ch == '\n':
            self.line+=1
            self.col = 1
        
    
    def get_until(self, stop, include_stop_char=False):
        chars = []
        ch = self.buffer[self.pos]
        while self.pos < len(self.buffer) and not stop(self.buffer[self.pos]):
            ch = self.buffer[self.pos]
            chars.append(ch)
            self.update_pos(ch)
        
        if self.pos < len(self.buffer) and include_stop_char:
            chars.append(self.buffer[self.pos])
            self.update_pos(self.buffer[self.pos])
            
        return ''.join(chars)            
        
    def at_end(self):
        return self.pos >= len(self.buffer)
        
    def lex(self):
        while not self.at_end():
            ch = self.buffer[self.pos]
            l,c = self.line, self.col
            self.update_pos(ch)

            if ch.isspace():
                pass
            #Comments
            elif ch == COMMENTSTART:
                self.get_until(lambda c: c == COMMENTEND)
            #Double char tokens...
            elif ch == ':' and self.buffer[self.pos-1:self.pos+1] == DOUBLESEMI:
                self.update_pos(self.buffer[self.pos])
                return Token(DOUBLESEMI, DOUBLESEMI, l, c)
            elif ch == '|' and self.buffer[self.pos-1:self.pos+1] == DOUBLEPIPE:
                self.update_pos(self.buffer[self.pos])
                return Token(DOUBLEPIPE, DOUBLEPIPE, l, c)
            #Single char tokens...
            elif ch in (DOT, LPAREN, RPAREN, LANGLE, RANGLE, COMMA, PLUS, MINUS, AT, ASTERISK, DIVIDE, PIPE):           
                return Token(ch, ch, l, c)
            elif ch == BANG: #BOUNDVARIABLE
                val = self.get_until(lambda c: not c.isalnum())
                if len(val) == 0:
                    raise ParseError('(%d, %d) Expecting variable name after !' % (l,c))
                elif val[0].isdigit():
                    raise ParseError('(%d, %d) Bound variable name cannot start with a digit' % (l,c+1))
                elif val[0].isupper():
                    raise ParseError('(%d, %d) Location cannot be bound with !' % (l,c+1))
                return Token(val, BOUNDVARIABLE, l,c)
            #Lower case words
            elif ch.isalpha() and ch.islower():
                val = ch + self.get_until(lambda c: not c.isalnum())
                if val in (IN, OUT, READ):
                    return Token(val, val, l,c)
                return Token(val, VARIABLE, l,c)
            #Upper case words
            elif ch.isalpha() and ch.isupper():
                val = ch + self.get_until(lambda c: not c.isalnum())
                return Token(val, LOCATION, l,c)
            #Digits
            elif ch.isdigit():
                val = int(ch + self.get_until(lambda c: not c.isdigit()))
                return Token(val, NUMBER, l,c)
            else:
                raise ParseError('(%d, %d) Invalid token "%s")' % (l, c, ch))
                
        #If we reach here we are at the end
        return Token(EOF, EOF, self.line, self.col)
            
class ParseError(Exception):
    def __init__(self,msg):
        self.msg = msg

class Tuple(Process):
    def __init__(self, location, items):
        self.location = location
        self.items = items
    def __str__(self):
        return '%s::<%s>' % (self.location, ', '.join(str(i) for i in self.items))
    
    def Accept(self, visitor):
        visitor.Visit(self)
    def Compile(self, context):
        pass

class In(Action):
    def __new__(cls):
        return Action.__new__(cls, 'in')

    def __str__(self):
        return 'inaction'
    def Accept(self, visitor):
        visitor.Visit(self)
    def Compile(self, context):
        pass
    
        
class Out(Action):
    def __new__(cls):
        return Action.__new__(cls, 'out')

    def __str__(self):
        return 'out'
    def Accept(self, visitor):
        visitor.Visit(self)
    def Compile(self, context):
        pass

class Read(Action):
    def __new__(cls):
        return Action.__new__(cls, 'read')

    def __str__(self):
        return 'read'
    def Accept(self, visitor):
        visitor.Visit(self)
    def Compile(self, context):
        pass

class Parser:
    def __init__(self, lexer):
        self.lexer = lexer
        self.t = None
        self.la = self.lexer.lex()
        self.errors = []
        self.procnames = []
        
    def get(self):
        self.t = self.la
        self.la = self.lexer.lex()

    def parse(self):
        system = ProcessSystem()
        while self.la.type != EOF:
            try:
                for procdef in self.net():
                    system.Add(procdef)
            except ParseError, ex: #Try to recover
                self.errors.append(ex.msg)
                print ex.msg

                #Try to resync to the next located item
                while self.t.type not in (EOF, DOUBLEPIPE):
                    self.get()
        return system

    def net(self):
        return self.list_of(self.located_item, DOUBLEPIPE)

    def expect_t(self, *expected):
        self.get()
        self.expect(self.t, *expected)
        
    def expect(self, token, *expected):
        if all(token.type != exp for exp in expected):
            if len(expected) == 1:
                raise ParseError("(%d, %d) Expected %s" % (token.line, token.col, expected[0]))
            elif len(expected) > 1:
                raise ParseError("(%d, %d) Expected %s or %s" % (token.line, token.col, ', '.join(expected[:-1]), expected[-1]))
    
    def located_item(self):
        self.expect_t(LOCATION)
        loc = self.t.val
        self.expect_t(DOUBLESEMI)
        if self.la.type == LANGLE:
            proc = Tuple(loc, self.tuple())
            for i in range(1,9999):
                name = loc + '_Tuple_' + str(i)
                if not name in self.procnames:
                    self.procnames.append(name)
                    return ProcessDefinition(proc, name, True)
        else:
            proc = self.process()
            for i in range(1,9999):
                name = loc + str(i)
                if not name in self.procnames:
                    self.procnames.append(name)
                    return ProcessDefinition(proc, name, True)
                    
            
        
    def tuple(self):
        return self.list_of(self.constant, COMMA, LANGLE, RANGLE)
    
    def process(self):
        if self.la.type == LPAREN:
            self.get()
            proc = self.non_deterministic_choice()
            self.expect_t(RPAREN)
            return proc
        elif self.la.val == 0:
            self.get()
            return NilProcess()
        else:
            return self.non_deterministic_choice()
    
    def non_deterministic_choice(self):
        procs = self.list_of(self.parallel_composition, PLUS)
        if len(procs) == 1:
            return procs[0]
        else:
            pc = NonDeterministicChoice()
            for p in procs:
                pc.Add(p)
            return pc
            
    def parallel_composition(self):
        procs = self.list_of(self.action_prefix, PIPE)
        if len(procs) == 1:
            return procs[0]
        else:
            pc = ParallelComposition()
            for p in procs:
                pc.Add(p)
            return pc
            
    def action_prefix(self):
        act = self.action()
        self.expect_t(DOT)
        proc = self.process()
        return ActionPrefix(act, proc)

    def action(self):
        self.expect_t(IN, OUT, READ)
        name = self.t.val
        if self.t.type in (IN,READ):
            expressions = self.list_of(self.input_expression, COMMA, LPAREN, RPAREN)
        else:
            expressions = self.list_of(self.expression, COMMA, LPAREN, RPAREN)
        self.expect_t(AT)
        self.expect_t(LOCATION, VARIABLE)
        if name == 'in':
            return In()
        elif name == 'out':
            return Out()
        elif name == 'read':
            return Read()
    
    def input_expression(self):
        self.expect(self.la, BOUNDVARIABLE, VARIABLE, LOCATION, NUMBER)
        if self.la.type == BOUNDVARIABLE:
            self.get()
            return self.t.val
        else:   
            return self.expression()
        
    def expression(self):
        self.expect(self.la, VARIABLE, LOCATION, NUMBER)
        if self.la.type in (LOCATION, NUMBER):
            return self.constant()
        self.expect_t(VARIABLE)
        return self.t.val

    def constant(self):
        self.expect_t(LOCATION, NUMBER)
        if self.t.type == LOCATION:
            return self.t.val
        elif self.t.type == NUMBER:
            return self.t.val

    
    def list_of(self, func, seperator=COMMA, startTok=None, endTok=None):
        if startTok:
            self.expect_t(startTok)
        items = []
        items.append(func())
        while self.la.type == seperator:
            self.get()
            items.append(func())
            
        if endTok:
            self.expect_t(endTok)
        
        return items



def compile_tuplespaces(context):
    print 'Compiling tuplespaces'
    
    tb = context.Module.DefineType("TupleSpaces", TypeAttributes.Public | TypeAttributes.Class)
    
    #The dictionary holding the 
    field = tb.DefineField("items", type(Dictionary[str, List[Object]]), FieldAttributes.Private | FieldAttributes.Static)
    
    staticcon = tb.DefineConstructor(MethodAttributes.Static, CallingConventions.Standard, Array[Type](()))
    il = staticcon.GetILGenerator()
    loclist = il.DeclareLocal(type(List[Object]))
    created_locations = []
    for pd in system:
        if type(pd.Process) == Tuple:
            print 'Compiling tuple', pd.Process
            

    il.Emit(OpCodes.Ret)
    tb.CreateType()
        
if __name__ == '__main__':
    text = """
    Foo::<23, Foj, 3>
    || FAS::<Con1, Con2>
    || John::
       read( !x23ASD)@J. in(1)@self . (out(x, y, John, 0)@a . 0) 
       | 
       read(a, F, 0)@x . 0 
       + out(1)@a .  0

    """
    p = Parser(Lexer(text))
    system = p.parse()
    options = CompileOptions.Parse(List[str]())
    options.OutputFile = r'c:\ee\klaimtest.exe'
    system.BeforeCompile += compile_tuplespaces
    system.Compile(options)
 