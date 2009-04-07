

#token types
LOCATION = 'LOCATION'
VARIABLE = 'VARIABLE'
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

class Parser:
    def __init__(self, lexer):
        self.lexer = lexer
        self.t = None
        self.la = self.lexer.lex()
        self.errors = []
        
    def get(self):
        self.t = self.la
        self.la = self.lexer.lex()

    def parse(self):
        while self.la.type != EOF:
            try:
                self.net()
            except ParseError, ex: #Try to recover
                self.errors.append(ex.msg)
                print ex.msg

                #Try to resync to the next located item
                while self.t.type not in (EOF, '||'):
                    self.get()
            

    def net(self):
        self.located_item()

        self.expect(self.la, DOUBLEPIPE, EOF)
        while self.la.type == DOUBLEPIPE:
            self.get()
            self.located_item()
        self.expect(self.la, EOF)

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
        self.expect_t(DOUBLESEMI)
        if self.la.type == LANGLE:
            self.tuple()
        else:
            self.process()
        
    def tuple(self):
        self.expect_t(LANGLE)
        self.atom()
        self.expect(self.la, COMMA, RANGLE)
        while self.la.type == COMMA:
            self.get()
            self.atom()
        self.expect_t(RANGLE)
    
    def process(self):
        if self.la.type == LPAREN:
            self.get()
            self.non_deterministic_choice()
            self.expect_t(RPAREN)
        elif self.la.val == 0:
            self.get()
        else:
            self.non_deterministic_choice()

    def non_deterministic_choice(self):
        self.parallel_composition()
        while self.la.type == PLUS:
            self.get()
            self.parallel_composition()
            
    def parallel_composition(self):
        self.action_prefix()
        while self.la.type == PIPE:
            self.get()
            self.action_prefix()
            
    def action_prefix(self):
        self.action()
        self.expect_t(DOT)
        self.process()

    def action(self):
        self.expect_t(IN, OUT, READ)
        self.expect_t(LPAREN)
        self.element()
        while self.la.type == COMMA:
            self.get()
            self.element()
        self.expect_t(RPAREN)
        self.expect_t(AT)
        self.expect_t(LOCATION, VARIABLE)
    
    def expression(self):
        #TODO: arithmetic expressions
        self.atom()

    def atom(self):
        self.expect_t(VARIABLE, LOCATION, NUMBER)
        if self.t.type == VARIABLE:
            pass #process var
        elif self.t.type == LOCATION:
            pass #process loc
        elif self.t.type == NUMBER:
            pass #process number
    
    def list_of(self, func, seperator=COMMA):
        list items = []
        items.append(func())
        while self.la.type == seperator:
            self.get()
            items.append(func())
        
        
if __name__ == '__main__':
    text = """
    Foo::<23, Foj, j>
    || FAS::<asdf, s>
    || John::read( a)@J. in(1)@self . (out(x, y, John, 0)@a . 0) | read(a, F, 0)@x . 0 + out(1)@a .  0

    """
    p = Parser(Lexer(text))
    p.parse()
#    tok = Token('begin','begin',21,3)
#    while tok:
#        print tok.val, ' type: ', tok.type, ' (%d,%d)' % (tok.line,tok.col)
#        tok = l.lex()
 